using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sketch_tale.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public int DurationDays { get; set; }
    public int TrialDays { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

