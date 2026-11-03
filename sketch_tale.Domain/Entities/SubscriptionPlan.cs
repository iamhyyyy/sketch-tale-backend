using sketch_tale.Domain.Common;

namespace sketch_tale.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public decimal Price { get; set; }
    public int ChildProfileLimit { get; set; }
    public int MonthlyCreditLimit { get; set; }
    public bool AccessFullStories { get; set; } = false;
    public bool CanExportStory { get; set; } = false;
    public int DurationDays { get; set; }
    public int FreeTrialDays { get; set; } = 0;
    public bool HaveParentProfileSub { get; set; } = false;
    public bool IsActive { get; set; } = true;
}

