
using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class Character : BaseEntity
{
    public Guid ChildProfileId { get; set; }
    public Guid DrawingId { get; set; }
    public Guid CharTypeId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string DefaultPronoun { get; set; } = string.Empty;
    public string? ProcessedSpriteUrl { get; set; }
    public string? AIDetectedTagsJson { get; set; }
    public CommonStatus Status { get; set; }
    public ParentApprovalStatus ParentApprovalStatus { get; set; }

    // Navigation Properties
    public ChildProfile Child { get; set; } = null!;
    public Drawing Drawing { get; set; } = null!;
    public CharType CharType { get; set; } = null!;
}
