using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sketch_tale.Domain.Entities;

public class PaymentTransaction : BaseEntity
{
    public Guid SubscriptionId { get; set; }
    public Guid ParentProfileId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string PaymentProvider { get; set; } = "PayOS";

    public string TxnRef { get; set; } = string.Empty; 
    public string TransactionNo { get; set; } = string.Empty;   
    public string BankCode { get; set; } = string.Empty;             
    public string ResponseCode { get; set; } = string.Empty;         
    public DateTime? PayDate { get; set; }      
    public string RawIpnResponse { get; set; } = string.Empty;       
}
