
using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class Character : BaseEntity
{
    public Guid ChildProfileId { get; set; }
    public Guid AICharacterGenerateId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string DefaultPronoun { get; set; } = string.Empty;
    public string FinalImageUrl { get; set; } = null!;
    public CommonStatus Status { get; set; }
    public ParentApprovalStatus ParentApprovalStatus { get; set; }
    public bool IsFavorite { get; set; } = false;
    public bool IsHidden { get; set; } = false;

    // Navigation Properties
    public ChildProfile Child { get; set; } = null!;
    public AICharacterGenerate AICharacterGenerate { get; set; } = null!;
}
