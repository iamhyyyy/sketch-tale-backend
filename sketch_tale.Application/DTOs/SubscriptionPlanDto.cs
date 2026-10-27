using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.DTOs;

public class SubscriptionPlanDto : BaseFieldDto
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public decimal Price { get; set; }
    public int ChildProfileLimit { get; set; }
    public int MonthlyCreditLimit { get; set; }
    public bool AccessFullStories { get; set; }
    public bool CanExportStory { get; set; }
    public bool HaveParentProfileSub { get; set; }
    public int? DurationDays { get; set; }
    public int? FreeTrialDays { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSubscriptionPlanDto
{
    public required string Name { get; set; }
    public required string Code { get; set; }
    public decimal Price { get; set; }
    public int ChildProfileLimit { get; set; }
    public int MonthlyCreditLimit { get; set; }
    public bool AccessFullStories { get; set; }
    public bool CanExportStory { get; set; }
    public bool HaveParentProfileSub { get; set; }
    public int? DurationDays { get; set; }
    public int? FreeTrialDays { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateSubscriptionPlanDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int ChildProfileLimit { get; set; }
    public int MonthlyCreditLimit { get; set; }
    public bool AccessFullStories { get; set; }
    public bool CanExportStory { get; set; }
    public bool HaveParentProfileSub { get; set; }
    public int? DurationDays { get; set; }
    public int? FreeTrialDays { get; set; }
    public bool IsActive { get; set; }
}
