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

        // 2. Webhook nhận kết quả tự động từ PayOS
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOsWebhook()
        {
            using var reader = new StreamReader(Request.Body);
            var jsonBody = await reader.ReadToEndAsync();

            var success = await _payOsService.ProcessWebhookAsync(jsonBody);
            return Ok(new { success });
        }

        // 3. Tra cứu thông tin thanh toán theo orderCode trực tiếp từ PayOS SDK
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

        // 4. Hủy thanh toán
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

                // Kiểm tra quyền: Chỉ phụ huynh sở hữu giao dịch này hoặc Admin mới có quyền hủy
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

        // 6. Admin xem toàn bộ lịch sử giao dịch (backward compatibility)
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
                    .Select(t => t.ParentProfile!.UserId)
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

        // 7. Tra cứu chi tiết 1 giao dịch từ DB theo orderCode (kèm auto-reconciliation khi redirect về)
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

                // Tự động đối soát (Reconciliation) nếu DB vẫn đang Pending
                if (transaction.Status == PaymentStatus.Pending && long.TryParse(orderCode, out long parsedOrderCode))
                {
                    try
                    {
                        var payOsInfo = await _payOsService.GetPaymentInfoAsync(parsedOrderCode);
                        var statusProp = payOsInfo?.GetType().GetProperty("Status")?.GetValue(payOsInfo)?.ToString()
                                      ?? payOsInfo?.GetType().GetProperty("status")?.GetValue(payOsInfo)?.ToString();

                        if (string.Equals(statusProp, "PAID", StringComparison.OrdinalIgnoreCase))
                        {
                            transaction.Status = PaymentStatus.Success;
                            transaction.ResponseCode = "00";
                            transaction.PayDate = DateTime.UtcNow.AddHours(7);
                            _unitOfWork.PaymentTransactionRepository.Update(transaction);

                            await _subscriptionService.UpdateSubscriptionForSuccessPayment(
                                transaction.SubscriptionId,
                                SubscriptionStatus.Active
                            );
                            await _unitOfWork.CompleteAsync();
                        }
                        else if (string.Equals(statusProp, "CANCELLED", StringComparison.OrdinalIgnoreCase))
                        {
                            transaction.Status = PaymentStatus.Failed;
                            transaction.ResponseCode = "CANCELLED";
                            transaction.PayDate = DateTime.UtcNow.AddHours(7);
                            _unitOfWork.PaymentTransactionRepository.Update(transaction);

                            await _subscriptionService.Delete(transaction.SubscriptionId);
                            await _unitOfWork.CompleteAsync();
                        }
                    }
                    catch
                    {
                        // Giữ nguyên dữ liệu hiện có trong DB nếu gọi sang PayOS bị gián đoạn mạng
                    }
                }

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

        // ─── ADMIN MANAGE APIS ──────────────────────────────────────────────────

        // 8. Admin quản lý danh sách giao dịch: Phân trang, Lọc theo trạng thái/thời gian/gói, Tìm kiếm
        [HttpGet("admin/transactions")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> GetAdminTransactions([FromQuery] PaymentFilterDto filter)
        {
            try
            {
                var transactions = await _unitOfWork.PaymentTransactionRepository
                    .GetAllWithDetailsAsync();

                var query = transactions.AsEnumerable();

                // Lọc theo trạng thái
                if (filter.Status.HasValue)
                {
                    query = query.Where(t => t.Status == filter.Status.Value);
                }

                // Lọc theo gói
                if (filter.SubscriptionPlanId.HasValue && filter.SubscriptionPlanId.Value != Guid.Empty)
                {
                    query = query.Where(t => t.Subscription?.SubscriptionPlanId == filter.SubscriptionPlanId.Value);
                }

                // Lọc theo khoảng ngày
                if (filter.FromDate.HasValue)
                {
                    query = query.Where(t => t.CreatedAt >= filter.FromDate.Value);
                }
                if (filter.ToDate.HasValue)
                {
                    query = query.Where(t => t.CreatedAt <= filter.ToDate.Value);
                }

                var filteredList = query.ToList();

                var parentUserIds = filteredList
                    .Where(t => t.ParentProfile != null)
                    .Select(t => t.ParentProfile!.UserId)
                    .Distinct()
                    .ToList();

                var users = await _userManager.Users
                    .Where(u => parentUserIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id);

                // Tìm kiếm theo Search keyword (orderCode, transactionNo, email, họ tên)
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var keyword = filter.Search.Trim().ToLower();
                    filteredList = filteredList.Where(t =>
                    {
                        var orderCodeMatch = !string.IsNullOrEmpty(t.TxnRef) && t.TxnRef.ToLower().Contains(keyword);
                        var transNoMatch = !string.IsNullOrEmpty(t.TransactionNo) && t.TransactionNo.ToLower().Contains(keyword);

                        User? u = null;
                        if (t.ParentProfile != null)
                        {
                            users.TryGetValue(t.ParentProfile.UserId, out u);
                        }
                        var emailMatch = u != null && !string.IsNullOrEmpty(u.Email) && u.Email.ToLower().Contains(keyword);
                        var nameMatch = u != null && ($"{u.FirstName} {u.LastName}".ToLower().Contains(keyword));

                        return orderCodeMatch || transNoMatch || emailMatch || nameMatch;
                    }).ToList();
                }

                var totalCount = filteredList.Count;
                var pageIndex = filter.PageIndex > 0 ? filter.PageIndex : 1;
                var pageSize = filter.PageSize > 0 ? filter.PageSize : 10;

                var pagedItems = filteredList
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t =>
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
                    })
                    .ToList();

                return Ok(new PagedResultDto<AdminPaymentTransactionDto>
                {
                    Items = pagedItems,
                    TotalCount = totalCount,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 9. Admin xem chi tiết kỹ thuật 1 giao dịch
        [HttpGet("admin/transactions/{id}")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> GetAdminTransactionById(Guid id)
        {
            try
            {
                var transaction = await _unitOfWork.PaymentTransactionRepository
                    .GetByIdWithDetailsAsync(id);

                if (transaction == null)
                    return NotFound(new { message = $"Không tìm thấy giao dịch với ID {id}." });

                User? user = null;
                if (transaction.ParentProfile != null)
                {
                    user = await _userManager.FindByIdAsync(transaction.ParentProfile.UserId.ToString());
                }

                var result = new AdminPaymentTransactionDetailDto
                {
                    Id                    = transaction.Id,
                    ParentProfileId       = transaction.ParentProfileId,
                    UserId                = transaction.ParentProfile?.UserId,
                    ParentEmail           = user?.Email,
                    ParentFullName        = user != null ? $"{user.FirstName} {user.LastName}".Trim() : null,
                    OrderCode             = transaction.TxnRef,
                    TransactionNo         = transaction.TransactionNo,
                    BankCode              = transaction.BankCode,
                    RawIpnResponse        = transaction.RawIpnResponse,
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

        // 10. Admin đồng bộ thủ công trạng thái giao dịch từ PayOS
        [HttpPost("admin/sync/{orderCode}")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> SyncPaymentStatus(long orderCode)
        {
            try
            {
                var transaction = await _unitOfWork.PaymentTransactionRepository
                    .GetByOrderCodeAsync(orderCode.ToString());

                if (transaction == null)
                    return NotFound(new { message = $"Không tìm thấy giao dịch với mã {orderCode}." });

                var payOsInfo = await _payOsService.GetPaymentInfoAsync(orderCode);
                var statusStr = payOsInfo?.GetType().GetProperty("Status")?.GetValue(payOsInfo)?.ToString()
                             ?? payOsInfo?.GetType().GetProperty("status")?.GetValue(payOsInfo)?.ToString()
                             ?? string.Empty;

                if (string.Equals(statusStr, "PAID", StringComparison.OrdinalIgnoreCase))
                {
                    transaction.Status = PaymentStatus.Success;
                    transaction.ResponseCode = "00";
                    transaction.PayDate = DateTime.UtcNow.AddHours(7);
                    _unitOfWork.PaymentTransactionRepository.Update(transaction);

                    await _subscriptionService.UpdateSubscriptionForSuccessPayment(
                        transaction.SubscriptionId,
                        SubscriptionStatus.Active
                    );
                    await _unitOfWork.CompleteAsync();
                }
                else if (string.Equals(statusStr, "CANCELLED", StringComparison.OrdinalIgnoreCase))
                {
                    transaction.Status = PaymentStatus.Failed;
                    transaction.ResponseCode = "CANCELLED";
                    transaction.PayDate = DateTime.UtcNow.AddHours(7);
                    _unitOfWork.PaymentTransactionRepository.Update(transaction);

                    await _subscriptionService.Delete(transaction.SubscriptionId);
                    await _unitOfWork.CompleteAsync();
                }

                return Ok(new
                {
                    message = "Đã đồng bộ trạng thái giao dịch từ PayOS thành công.",
                    payOsStatus = statusStr,
                    dbStatus = transaction.Status.ToString()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 11. Admin thống kê doanh thu và số lượng giao dịch
        [HttpGet("admin/stats")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> GetPaymentStats()
        {
            try
            {
                var transactions = await _unitOfWork.PaymentTransactionRepository
                    .GetAllWithDetailsAsync();

                var now = DateTime.UtcNow.AddHours(7);
                var today = now.Date;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);

                var successTransactions = transactions.Where(t => t.Status == PaymentStatus.Success).ToList();

                var stats = new PaymentStatsDto
                {
                    TotalRevenue = successTransactions.Sum(t => t.Amount),
                    RevenueToday = successTransactions.Where(t => t.CreatedAt.Date == today).Sum(t => t.Amount),
                    RevenueThisMonth = successTransactions.Where(t => t.CreatedAt >= startOfMonth).Sum(t => t.Amount),
                    TotalTransactions = transactions.Count,
                    SuccessfulTransactions = successTransactions.Count,
                    FailedTransactions = transactions.Count(t => t.Status == PaymentStatus.Failed),
                    PendingTransactions = transactions.Count(t => t.Status == PaymentStatus.Pending)
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
