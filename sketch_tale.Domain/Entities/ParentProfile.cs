using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sketch_tale.Domain.Entities;

public class ParentProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SubscriptionPlanId { get; set; } = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public int RemainingChildProfile { get; set; } = 1;
    public int RemainingMonthlyCredit { get; set; } = 500;
    public bool TrialPlusPlan { get; set; } = false;

    public ParentProfileSub? ParentProfileSub { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    public ICollection<ChildProfile> ChildProfiles { get; set; } = new List<ChildProfile>();
}
