using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace sketch_tale.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid ParentProfileId { get; set; }
    public Guid PlanId { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.PendingPayment;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Navigation
    public SubscriptionPlan Plan { get; set; } = null!;
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
}
