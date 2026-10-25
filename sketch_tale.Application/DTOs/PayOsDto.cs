using sketch_tale.Domain.Enums;
using System;

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
}
