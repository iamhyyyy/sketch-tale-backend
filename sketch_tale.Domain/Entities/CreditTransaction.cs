
using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class CreditTransaction : BaseEntity
{
    public Guid ParentProfileId { get; set; }

    public int Amount { get; set; }
    public CreditActionType ActionType { get; set; }
    public string Description { get; set; } = string.Empty;

    // Navigation Properties
    public ParentProfile Parent { get; set; } = null!;
}
