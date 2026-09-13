using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class Drawing : BaseEntity
{
    public Guid ChildId { get; set; }
    public string OriginalImageUrl { get; set; } = string.Empty;
    public DrawingType DrawingType { get; set; }

    // Navigation Properties
    public ChildProfile Child { get; set; } = null!;
    public ICollection<Character> Characters { get; set; } = new List<Character>();
}
