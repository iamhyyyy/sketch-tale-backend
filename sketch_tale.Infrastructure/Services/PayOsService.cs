using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace sketch_tale.Infrastructure.Services
{
    public class PayOsService : IPayOsService
    {
        private readonly PayOSClient _payOS;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISubscriptionService _subscriptionService;

        public PayOsService(
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            ISubscriptionService subscriptionService)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _subscriptionService = subscriptionService;

            _payOS = new PayOSClient(
                clientId:    configuration["PayOS:ClientId"] ?? string.Empty,
                apiKey:      configuration["PayOS:ApiKey"] ?? string.Empty,
                checksumKey: configuration["PayOS:ChecksumKey"] ?? string.Empty
            );
        }

        // ─── API 1: Tạo link thanh toán mua gói cước Subscription ───────────────────
        public async Task<string> CreatePaymentLinkAsync(Guid parentProfileId, CreatePaymentDto dto)
        {
            // 1. Kiểm tra gói Plan có tồn tại và đang hoạt động không
            var plan = await _unitOfWork.SubscriptionPlanRepository.GetByIdAsync(dto.SubscriptionPlanId);
            if (plan == null || !plan.IsActive)
                throw new Exception("Gói đăng ký không tồn tại hoặc đã ngừng kích hoạt.");

            var parent = await _unitOfWork.ParentProfileRepository.GetByIdAsync(parentProfileId)
                ?? await _unitOfWork.ParentProfileRepository.GetByUserIdAsync(parentProfileId);
            if (parent == null)
                throw new Exception("Không tìm thấy thông tin tài khoản phụ huynh.");

            parentProfileId = parent.Id;

            // 2. Tạo bản ghi Subscription ở trạng thái PendingPayment
            var now = DateTime.UtcNow.AddHours(7);
            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                ParentProfileId = parentProfileId,
                SubscriptionPlanId = plan.Id,
                Status = SubscriptionStatus.PendingPayment,
                StartDate = now,
                EndDate = now.AddDays(plan.DurationDays)
            };
            await _unitOfWork.SubscriptionRepository.AddAsync(subscription);

            // 3. Tạo orderCode duy nhất (long) = timestamp ms * 10 + random digit
            var random = new Random();
            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 10 + random.Next(0, 10);

            // 4. Tạo PaymentTransaction lưu vết thanh toán
            var transaction = new sketch_tale.Domain.Entities.PaymentTransaction
            {
                Id = Guid.NewGuid(),
                SubscriptionId = subscription.Id,
                ParentProfileId = parentProfileId,
                Amount = plan.Price,
                Status = PaymentStatus.Pending,
                PaymentProvider = "PayOS",
                TxnRef = orderCode.ToString()
            };
            await _unitOfWork.PaymentTransactionRepository.AddAsync(transaction);
            await _unitOfWork.CompleteAsync();

            // 5. Build description: PayOS giới hạn 25 ký tự, không dấu tiếng Việt
            var description = RemoveDiacritics(dto.Description ?? $"Mua {plan.Name}");
            if (description.Length > 25) description = description[..25];

            var amount = (int)plan.Price;

            // 6. Build request theo PayOS SDK
            var request = new CreatePaymentLinkRequest
            {
                OrderCode   = orderCode,
                Amount      = amount,
                Description = description,
                CancelUrl   = _configuration["PayOS:CancelUrl"] ?? "http://localhost:5173/payment/cancel",
                ReturnUrl   = _configuration["PayOS:ReturnUrl"] ?? "http://localhost:5173/payment/success",
                BuyerName   = dto.BuyerName,
                BuyerEmail  = dto.BuyerEmail,
                Items       = new List<PaymentLinkItem>
                {
                    new PaymentLinkItem
                    {
                        Name     = RemoveDiacritics(plan.Name),
                        Quantity = 1,
                        Price    = amount
                    }
                }
            };

            // 7. Gọi PayOS API lấy checkoutUrl
            var result = await _payOS.PaymentRequests.CreateAsync(request);
            return result.CheckoutUrl;
        }

        // ─── API 2: Xử lý Webhook (Server-to-Server từ PayOS) ────────────────────────
        public async Task<bool> ProcessWebhookAsync(string jsonBody)
        {
            try
            {
                // Bước 1: Deserialize JSON body
                var webhookPayload = JsonSerializer.Deserialize<Webhook>(jsonBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (webhookPayload == null) return false;

                // Bước 2: Verify chữ ký HMAC
                var data = await _payOS.Webhooks.VerifyAsync(webhookPayload);
                if (data == null) return false;

                // Bước 3: Tìm PaymentTransaction theo orderCode (đã lưu trong TxnRef)
                var transaction = await _unitOfWork.PaymentTransactionRepository.GetByOrderCodeAsync(data.OrderCode.ToString());
                if (transaction == null) return false;

                // Nếu giao dịch đã hoàn tất trước đó thì bỏ qua (Idempotency)
                if (transaction.Status == PaymentStatus.Success) return true;

                // Bước 4: Kiểm tra mã kết quả PayOS
                if (data.Code == "00") // Thanh toán thành công
                {
                    transaction.Status = PaymentStatus.Success;
                    transaction.ResponseCode = data.Code;
                    transaction.TransactionNo = data.Reference ?? string.Empty;
                    transaction.PayDate = DateTime.UtcNow.AddHours(7);
                    transaction.RawIpnResponse = jsonBody;

                    // KÍCH HOẠT SUBSCRIPTION VÀ CẬP NHẬT QUYỀN LỢI CHO PHỤ HUYNH
                    await _subscriptionService.UpdateSubscriptionForSuccessPayment(
                        transaction.SubscriptionId, 
                        SubscriptionStatus.Active
                    );
                }
                else // Thanh toán thất bại hoặc hủy
                {
                    transaction.Status = PaymentStatus.Failed;
                    transaction.ResponseCode = data.Code;
                    transaction.RawIpnResponse = jsonBody;
                }

                _unitOfWork.PaymentTransactionRepository.Update(transaction);
                await _unitOfWork.CompleteAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ─── API 3: Query trạng thái giao dịch ───────────────────────────────────────
        public async Task<object> GetPaymentInfoAsync(long orderCode)
        {
            return await _payOS.PaymentRequests.GetAsync(orderCode);
        }

        // ─── API 4: Hủy payment link ─────────────────────────────────────────────────
        public async Task<object> CancelPaymentAsync(long orderCode)
        {
            return await _payOS.PaymentRequests.CancelAsync(orderCode);
        }

        // ─── Helper: Xóa dấu tiếng Việt (chuẩn PayOS) ────────────────────────────────
        private static string RemoveDiacritics(string text)
        {
            var map = new Dictionary<string, string>
            {
                {"à","a"},{"á","a"},{"â","a"},{"ã","a"},{"ả","a"},{"ạ","a"},
                {"ă","a"},{"ắ","a"},{"ằ","a"},{"ẳ","a"},{"ẵ","a"},{"ặ","a"},
                {"ấ","a"},{"ầ","a"},{"ẩ","a"},{"ẫ","a"},{"ậ","a"},
                {"è","e"},{"é","e"},{"ê","e"},{"ẹ","e"},{"ẻ","e"},{"ẽ","e"},
                {"ế","e"},{"ề","e"},{"ể","e"},{"ễ","e"},{"ệ","e"},
                {"ì","i"},{"í","i"},{"ỉ","i"},{"ĩ","i"},{"ị","i"},
                {"ò","o"},{"ó","o"},{"ô","o"},{"õ","o"},{"ỏ","o"},{"ọ","o"},
                {"ố","o"},{"ồ","o"},{"ổ","o"},{"ỗ","o"},{"ộ","o"},
                {"ơ","o"},{"ớ","o"},{"ờ","o"},{"ở","o"},{"ỡ","o"},{"ợ","o"},
                {"ù","u"},{"ú","u"},{"û","u"},{"ủ","u"},{"ũ","u"},{"ụ","u"},
                {"ư","u"},{"ứ","u"},{"ừ","u"},{"ử","u"},{"ữ","u"},{"ự","u"},
                {"ỳ","y"},{"ý","y"},{"ỷ","y"},{"ỹ","y"},{"ỵ","y"},{"đ","d"},
                {"À","A"},{"Á","A"},{"Â","A"},{"Ã","A"},{"Ả","A"},{"Ạ","A"},
                {"Ă","A"},{"Ắ","A"},{"Ằ","A"},{"Ẳ","A"},{"Ẵ","A"},{"Ặ","A"},
                {"È","E"},{"É","E"},{"Ê","E"},{"Ẹ","E"},{"Ẻ","E"},{"Ẽ","E"},
                {"Ế","E"},{"Ề","E"},{"Ể","E"},{"Ễ","E"},{"Ệ","E"},
                {"Ì","I"},{"Í","I"},{"Ỉ","I"},{"Ĩ","I"},{"Ị","I"},
                {"Ò","O"},{"Ó","O"},{"Ô","O"},{"Õ","O"},{"Ỏ","O"},{"Ọ","O"},
                {"Ố","O"},{"Ồ","O"},{"Ổ","O"},{"Ỗ","O"},{"Ộ","O"},
                {"Ơ","O"},{"Ớ","O"},{"Ờ","O"},{"Ở","O"},{"Ỡ","O"},{"Ợ","O"},
                {"Ù","U"},{"Ú","U"},{"Û","U"},{"Ủ","U"},{"Ũ","U"},{"Ụ","U"},
                {"Ư","U"},{"Ứ","U"},{"Ừ","U"},{"Ử","U"},{"Ữ","U"},{"Ự","U"},
                {"Ỳ","Y"},{"Ý","Y"},{"Ỷ","Y"},{"Ỹ","Y"},{"Ỵ","Y"},{"Đ","D"}
            };
            foreach (var kv in map)
                text = text.Replace(kv.Key, kv.Value);
            return text;
        }
    }
}
