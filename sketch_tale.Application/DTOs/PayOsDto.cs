using sketch_tale.Domain.Enums;
using System;
using System.Collections.Generic;

namespace sketch_tale.Application.DTOs
{
    public class CreatePaymentDto
    {
        public Guid SubscriptionPlanId { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerEmail { get; set; }
        public string? Description { get; set; }
    }

    public class PaymentTransactionDto
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;          // TxnRef
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string PaymentProvider { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public DateTime? PayDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public int DurationDays { get; set; }

        public string SubscriptionStatus { get; set; } = string.Empty;
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime SubscriptionEndDate { get; set; }
    }

    // ─── Output DTO: Lịch sử giao dịch (Admin) ────────────────────────────────
    public class AdminPaymentTransactionDto : PaymentTransactionDto
    {
        public Guid ParentProfileId { get; set; }
        public Guid? UserId { get; set; }
        public string? ParentEmail { get; set; }
        public string? ParentFullName { get; set; }
    }

    // ─── Chi tiết kỹ thuật giao dịch (Admin) ─────────────────────────────────
    public class AdminPaymentTransactionDetailDto : AdminPaymentTransactionDto
    {
        public string TransactionNo { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string RawIpnResponse { get; set; } = string.Empty;
    }

    // ─── Bộ lọc và phân trang quản lý giao dịch ───────────────────────────────
    public class PaymentFilterDto
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public PaymentStatus? Status { get; set; }
        public string? Search { get; set; } // Tìm theo orderCode, transactionNo, email, họ tên
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? SubscriptionPlanId { get; set; }
    }

    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 10));
    }

    // ─── Thống kê doanh thu và giao dịch (Admin Dashboard) ───────────────────
    public class PaymentStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueToday { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int TotalTransactions { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public int PendingTransactions { get; set; }
    }
}
