using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.DTOs;

public class CreditCostDto : BaseFieldDto
{
    public string FeatureKey { get; set; } = null!;
    public int Cost { get; set; }
    public string FeatureNameVi { get; set; } = null!;
    public string FeatureNameEn { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class CreateCreditCostDto
{
    public required string FeatureKey { get; set; }
    public int Cost { get; set; }
    public required string FeatureNameVi { get; set; }
    public required string FeatureNameEn { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateCreditCostDto
{
    public string FeatureKey { get; set; } = string.Empty;
    public int Cost { get; set; }
    public string FeatureNameVi { get; set; } = string.Empty;
    public string FeatureNameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
