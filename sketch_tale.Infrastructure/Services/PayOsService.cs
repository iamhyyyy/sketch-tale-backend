using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Application.Services;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace sketch_tale.Infrastructure.Services
{
    public class PayOsService : IPayOsService
    {
        private readonly PayOSClient _payOS;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public PayOsService(
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;

            _payOS = new PayOSClient(
                clientId:    configuration["PayOS:ClientId"] ?? string.Empty,
                apiKey:      configuration["PayOS:ApiKey"] ?? string.Empty,
                checksumKey: configuration["PayOS:ChecksumKey"] ?? string.Empty
            );
        }

        public async Task<string> CreatePaymentLinkAsync(Guid parentProfileId, CreatePaymentDto dto)
        {
            var plan = await _unitOfWork.SubscriptionPlanRepository.GetByIdAsync(dto.SubscriptionPlanId);
            if (plan == null || !plan.IsActive)
                throw new Exception("Gói đăng ký không tồn tại hoặc đã ngừng kích hoạt.");

            var parent = await _unitOfWork.ParentProfileRepository.GetByUserIdAsync(parentProfileId);
            if (parent == null)
                throw new Exception("Không tìm thấy thông tin tài khoản phụ huynh.");

            parentProfileId = parent.Id;
            var existingSubs = await _unitOfWork.SubscriptionRepository.GetByParentProfileId(parentProfileId);
            foreach (var pendingSub in existingSubs.Where(s => s.Status == SubscriptionStatus.PendingPayment))
            {
                pendingSub.Status = SubscriptionStatus.Cancelled;
                _unitOfWork.SubscriptionRepository.Update(pendingSub);
            }

            var existingPayment = await _unitOfWork.PaymentTransactionRepository.GetByParentProfileIdAsync(parentProfileId);
            foreach (var pendingPayment in existingPayment.Where(s => s.Status == PaymentStatus.Pending))
            {
                if (await CancelPaymentAsync(long.Parse(pendingPayment.TxnRef)))
                {
                    pendingPayment.Status = PaymentStatus.Failed;
                    _unitOfWork.PaymentTransactionRepository.Update(pendingPayment);
                }
            }

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

            var random = new Random();
            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 10 + random.Next(0, 10);

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

            var description = RemoveDiacritics(dto.Description ?? $"Mua {plan.Name}");
            if (description.Length > 25) description = description[..25];

            var amount = (int)plan.Price;

            var request = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = amount,
                Description = description,
                CancelUrl = _configuration["PayOS:CancelUrl"],
                ReturnUrl = _configuration["PayOS:ReturnUrl"],
                BuyerName = dto.BuyerName,
                BuyerEmail = dto.BuyerEmail,
                Items = new List<PaymentLinkItem>
                {
                    new PaymentLinkItem
                    {
                        Name     = RemoveDiacritics(plan.Name),
                        Quantity = 1,
                        Price    = amount
                    }
                }
            };
            var result = await _payOS.PaymentRequests.CreateAsync(request);
            return result.CheckoutUrl;
        }

        public async Task<bool> ProcessWebhookAsync(string jsonBody)
        {
            try
            {
                var webhookPayload = JsonSerializer.Deserialize<Webhook>(jsonBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (webhookPayload == null) return false;

                var data = await _payOS.Webhooks.VerifyAsync(webhookPayload);
                if (data == null) return false;

                var transaction = await _unitOfWork.PaymentTransactionRepository.GetByOrderCodeAsync(data.OrderCode.ToString());
                if (transaction == null) return false;

                if (transaction.Status == PaymentStatus.Success) return true;

                var subription = await _unitOfWork.SubscriptionRepository.GetByIdAsync(transaction.SubscriptionId);

                if (data.Code == "00") 
                {
                    transaction.Status = PaymentStatus.Success;
                    transaction.ResponseCode = data.Code;
                    transaction.TransactionNo = data.Reference ?? string.Empty;
                    transaction.PayDate = DateTime.UtcNow.AddHours(7);
                    transaction.RawIpnResponse = jsonBody;

                    subription.Status = SubscriptionStatus.Active;
                    
                }
                else
                {
                    transaction.Status = PaymentStatus.Failed;
                    transaction.ResponseCode = data.Code;
                    transaction.RawIpnResponse = jsonBody;
                    transaction.PayDate = DateTime.UtcNow.AddHours(7);

                    subription.Status = SubscriptionStatus.Cancelled;
                }

                _unitOfWork.PaymentTransactionRepository.Update(transaction);
                _unitOfWork.SubscriptionRepository.Update(subription);
                await _unitOfWork.CompleteAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CancelPaymentAsync(long orderCode)
        {
            try
            {
                var paymentLink =
                    await _payOS.PaymentRequests.CancelAsync(orderCode);

                return paymentLink != null && paymentLink.Status.ToString().Equals("CANCELLED", StringComparison.OrdinalIgnoreCase) == true;
            }
            catch (Exception ex)
            {
                throw new Exception($"PayOS Error: {ex.Message}", ex);
            }
        }

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
