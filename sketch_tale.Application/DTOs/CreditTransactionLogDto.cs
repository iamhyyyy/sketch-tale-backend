using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.DTOs;

public class CreditTransactionLogDto : BaseFieldDto
{
    public Guid ParentProfileId { get; set; }

    public ActionType ActionType { get; set; }
    public string? FeatureKey { get; set; }
    public int Amount { get; set; }
    public int CreditBefore { get; set; }
    public int CreditAfter { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class CreateCreditTransactionLogDto
{
    public Guid ParentProfileId { get; set; }
    public ActionType ActionType { get; set; }
    public string? FeatureKey { get; set; }
    public int Amount { get; set; }
    public int CreditBefore { get; set; }
    public int CreditAfter { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class UpdateCreditTransactionLogDto
{
    public Guid ParentProfileId { get; set; }
    public ActionType ActionType { get; set; }
    public string? FeatureKey { get; set; }
    public int Amount { get; set; }
    public int CreditBefore { get; set; }
    public int CreditAfter { get; set; }
    public string Description { get; set; } = string.Empty;
}
