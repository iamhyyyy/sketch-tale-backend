
using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class CreditTransactionLog : BaseEntity
{
    public Guid ParentProfileId { get; set; }

    public ActionType ActionType { get; set; }
    public string? FeatureKey { get; set; }
    public int Amount { get; set; }
    public int CreditBefore { get; set; }
    public int CreditAfter { get; set; }
    
    public string Description { get; set; } = string.Empty;
}
