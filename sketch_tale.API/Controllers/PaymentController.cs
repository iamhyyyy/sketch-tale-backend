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
        private readonly IPaymentTransactionService _paymentService;
        public PaymentController(
            IPayOsService payOsService,
            IPaymentTransactionService paymentService)
        {
            _payOsService = payOsService;
            _paymentService = paymentService;
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


        [HttpPatch("cancel-paymentlink")]
        [Authorize]
        public async Task<IActionResult> CancelPayment([FromBody] CancelPaymentDto dto)
        {
            try
            {
                var transaction = await _paymentService.CancelPayment(dto.orderCode, dto.code);
                if(transaction)
                {
                    return Ok(new { message = "Đã hủy giao dịch thành công." });

                }
                return BadRequest(new { message = "Không thể hủy giao dịch." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
