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


        [HttpDelete("{orderCode}")]
        [Authorize]
        public async Task<IActionResult> CancelPayment(long orderCode)
        {
            try
            {
                var transaction = await _unitOfWork.PaymentTransactionRepository
                    .GetByOrderCodeAsync(orderCode.ToString());

                if (transaction == null)
                    return NotFound(new { message = $"Không tìm thấy giao dịch với mã đơn hàng {orderCode}." });

                var isAdmin = User.IsInRole("admin") || User.IsInRole("Admin");
                if (!isAdmin)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub")
                        ?? User.FindFirstValue("nameid");

                    if (string.IsNullOrEmpty(userId)) return Unauthorized();

                    var parent = await _unitOfWork.ParentProfileRepository.GetByUserIdAsync(Guid.Parse(userId));
                    if (parent == null || parent.Id != transaction.ParentProfileId)
                    {
                        return StatusCode(StatusCodes.Status403Forbidden, new { message = "Bạn không có quyền hủy giao dịch này." });
                    }
                }

                var result = await _payOsService.CancelPaymentAsync(orderCode);

                if (transaction.Status == PaymentStatus.Pending)
                {
                    transaction.Status = PaymentStatus.Failed;
                    transaction.ResponseCode = "CANCELLED";
                    transaction.PayDate = DateTime.UtcNow.AddHours(7);
                    _unitOfWork.PaymentTransactionRepository.Update(transaction);

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
    }
}
