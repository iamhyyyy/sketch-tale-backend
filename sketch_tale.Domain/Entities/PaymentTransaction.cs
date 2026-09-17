using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sketch_tale.Domain.Entities;

public class PaymentTransaction : BaseEntity
{
    public Guid SubscriptionId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string PaymentProvider { get; set; } = "VNPAY";

    // VNPay Data
    public string TxnRef { get; set; } = string.Empty; // vnp_TxnRef
    public string? TransactionNo { get; set; }   // vnp_TransactionNo
    public string? BankCode { get; set; }             // vnp_BankCode
    public string? ResponseCode { get; set; }         // vnp_ResponseCode
    public DateTime? PayDate { get; set; }      // Parsed from vnp_PayDate
    public string? RawIpnResponse { get; set; }       // Audit trail Log JSON

    // Navigation
    public Subscription Subscription { get; set; } = null!;
}
