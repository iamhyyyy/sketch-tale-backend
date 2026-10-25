using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace sketch_tale.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IPayOsService _payOsService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISubscriptionService _subscriptionService;
        private readonly UserManager<User> _userManager;

        public PaymentController(
            IPayOsService payOsService,
            IUnitOfWork unitOfWork,
            ISubscriptionService subscriptionService,
            UserManager<User> userManager)
        {
            _payOsService = payOsService;
            _unitOfWork = unitOfWork;
            _subscriptionService = subscriptionService;
            _userManager = userManager;
        }

        // 1. Phụ huynh tạo link thanh toán mua gói
        [HttpPost("create-checkout")]
        [Authorize(Roles = "parent,Parent")]
        public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub")
                    ?? User.FindFirstValue("nameid");

                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var checkoutUrl = await _payOsService.CreatePaymentLinkAsync(Guid.Parse(userId), dto);
                return Ok(new { checkoutUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOsWebhook()
        {
            using var reader = new StreamReader(Request.Body);
            var jsonBody = await reader.ReadToEndAsync();

            var success = await _payOsService.ProcessWebhookAsync(jsonBody);
            return Ok(new { success });
        }

        // 3. Tra cứu thông tin thanh toán theo orderCode (từ PayOS trực tiếp)
        [HttpGet("info/{orderCode}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentInfo(long orderCode)
        {
            try
            {
                var info = await _payOsService.GetPaymentInfoAsync(orderCode);
                return Ok(info);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

   
        [HttpDelete("{orderCode}")]
        [Authorize]
        public async Task<IActionResult> CancelPayment(long orderCode)
        {
            try
            {

                var result = await _payOsService.CancelPaymentAsync(orderCode);


                var transaction = await _unitOfWork.PaymentTransactionRepository
                    .GetByOrderCodeAsync(orderCode.ToString());

                if (transaction != null && transaction.Status == PaymentStatus.Pending)
                {
                    // Cập nhật PaymentTransaction → Failed
                    transaction.Status = PaymentStatus.Failed;
                    transaction.ResponseCode = "CANCELLED";
                    transaction.PayDate = DateTime.UtcNow.AddHours(7);
                    _unitOfWork.PaymentTransactionRepository.Update(transaction);

                    // Cập nhật Subscription → Cancelled
                    await _subscriptionService.Delete(transaction.SubscriptionId);

                    await _unitOfWork.CompleteAsync();
                }

                return Ok(new { message = "Đã hủy thanh toán thành công.", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 5. Phụ huynh xem lịch sử giao dịch của chính mình
        [HttpGet("history")]
        [Authorize(Roles = "parent,Parent")]
        public async Task<IActionResult> GetMyPaymentHistory()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub")
                    ?? User.FindFirstValue("nameid");

                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                // Resolve ParentProfile từ UserId (JWT claim)
                var parent = await _unitOfWork.ParentProfileRepository.GetByUserIdAsync(Guid.Parse(userId))
                    ?? await _unitOfWork.ParentProfileRepository.GetByIdAsync(Guid.Parse(userId));

                if (parent == null)
                    return NotFound(new { message = "Không tìm thấy thông tin phụ huynh." });

                var transactions = await _unitOfWork.PaymentTransactionRepository
                    .GetByParentProfileIdAsync(parent.Id);

                var result = transactions.Select(t => new PaymentTransactionDto
                {
                    Id                    = t.Id,
                    OrderCode             = t.TxnRef,
                    Amount                = t.Amount,
                    Status                = t.Status,
                    PaymentProvider       = t.PaymentProvider,
                    ResponseCode          = t.ResponseCode,
                    PayDate               = t.PayDate,
                    CreatedAt             = t.CreatedAt,
                    PlanName              = t.Subscription?.Plan?.Name ?? string.Empty,
                    DurationDays          = t.Subscription?.Plan?.DurationDays ?? 0,
                    SubscriptionStatus    = t.Subscription?.Status.ToString() ?? string.Empty,
                    SubscriptionStartDate = t.Subscription?.StartDate ?? default,
                    SubscriptionEndDate   = t.Subscription?.EndDate ?? default
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 6. Admin xem toàn bộ lịch sử giao dịch
        [HttpGet("history/all")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> GetAllPaymentHistory()
        {
            try
            {
                var transactions = await _unitOfWork.PaymentTransactionRepository
                    .GetAllWithDetailsAsync();

                var parentUserIds = transactions
                    .Where(t => t.ParentProfile != null)
                    .Select(t => t.ParentProfile.UserId)
                    .Distinct()
                    .ToList();

                var users = await _userManager.Users
                    .Where(u => parentUserIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id);

                var result = transactions.Select(t =>
                {
                    User? user = null;
                    if (t.ParentProfile != null)
                    {
                        users.TryGetValue(t.ParentProfile.UserId, out user);
                    }

                    return new AdminPaymentTransactionDto
                    {
                        Id                    = t.Id,
                        ParentProfileId       = t.ParentProfileId,
                        UserId                = t.ParentProfile?.UserId,
                        ParentEmail           = user?.Email,
                        ParentFullName        = user != null ? $"{user.FirstName} {user.LastName}".Trim() : null,
                        OrderCode             = t.TxnRef,
                        Amount                = t.Amount,
                        Status                = t.Status,
                        PaymentProvider       = t.PaymentProvider,
                        ResponseCode          = t.ResponseCode,
                        PayDate               = t.PayDate,
                        CreatedAt             = t.CreatedAt,
                        PlanName              = t.Subscription?.Plan?.Name ?? string.Empty,
                        DurationDays          = t.Subscription?.Plan?.DurationDays ?? 0,
                        SubscriptionStatus    = t.Subscription?.Status.ToString() ?? string.Empty,
                        SubscriptionStartDate = t.Subscription?.StartDate ?? default,
                        SubscriptionEndDate   = t.Subscription?.EndDate ?? default
                    };
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 7. Tra cứu chi tiết 1 giao dịch từ DB theo orderCode (dành cho FE hiển thị kết quả sau khi PayOS redirect về)
        [HttpGet("transaction/{orderCode}")]
        [Authorize]
        public async Task<IActionResult> GetTransactionByOrderCode(string orderCode)
        {
            try
            {
                var transaction = await _unitOfWork.PaymentTransactionRepository
                    .GetByOrderCodeAsync(orderCode);

                if (transaction == null)
                    return NotFound(new { message = $"Không tìm thấy giao dịch với mã đơn hàng {orderCode}." });

                var result = new PaymentTransactionDto
                {
                    Id                    = transaction.Id,
                    OrderCode             = transaction.TxnRef,
                    Amount                = transaction.Amount,
                    Status                = transaction.Status,
                    PaymentProvider       = transaction.PaymentProvider,
                    ResponseCode          = transaction.ResponseCode,
                    PayDate               = transaction.PayDate,
                    CreatedAt             = transaction.CreatedAt,
                    PlanName              = transaction.Subscription?.Plan?.Name ?? string.Empty,
                    DurationDays          = transaction.Subscription?.Plan?.DurationDays ?? 0,
                    SubscriptionStatus    = transaction.Subscription?.Status.ToString() ?? string.Empty,
                    SubscriptionStartDate = transaction.Subscription?.StartDate ?? default,
                    SubscriptionEndDate   = transaction.Subscription?.EndDate ?? default
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
