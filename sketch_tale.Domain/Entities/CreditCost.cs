using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class CreditCost : BaseEntity
{
    public string FeatureKey { get; set; } = null!;
    public int Cost { get; set; }
    public string FeatureNameVi { get; set; } = null!;
    public string FeatureNameEn { get; set; } = null!;
    public bool IsActive { get; set; }
}
