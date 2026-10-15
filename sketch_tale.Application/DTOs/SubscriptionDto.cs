using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using System.Text.Json.Serialization;

namespace sketch_tale.Application.DTOs;

public class SubscriptionDto : BaseFieldDto
{
    public Guid ParentProfileId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    [JsonPropertyOrder(1)]
    public SubscriptionPlanDto Plan { get; set; } = null!;
}

public class CreateSubscriptionDto
{
    public Guid ParentProfileId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
}

public class UpdateSubscriptionDto
{
    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
