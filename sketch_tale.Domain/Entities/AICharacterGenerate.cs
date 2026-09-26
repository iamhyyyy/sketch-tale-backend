
using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class AICharacterGenerate : BaseEntity
{
    public Guid ChildProfileId { get; set; }
    public Guid DrawingId { get; set; }
    public Guid CharTypeId { get; set; }

    public string RawImageUrl { get; set; } = null!;

    public string AIPromptUsed { get; set; } = null!;
    public string? GenerateImageUrl { get; set; }
    public string? AIDetectedTagsJson { get; set; }


    public AIGenStatus AIGenStatus { get; set; }
    public string? ErrorMessage { get; set; }

    // Navigation Properties
    public ChildProfile Child { get; set; } = null!;
    public Drawing Drawing { get; set; } = null!;
    public CharType CharType { get; set; } = null!;
}
