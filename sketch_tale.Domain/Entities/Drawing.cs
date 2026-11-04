using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class Drawing : BaseEntity
{
    public Guid ChildProfileId { get; set; }
    public string OriginalImageUrl { get; set; } = null!;
    public DrawingType DrawingType { get; set; }

    // Navigation Properties
    public ICollection<AICharacterGenerate> AICharacterGenerates { get; set; } = new List<AICharacterGenerate>();
}
