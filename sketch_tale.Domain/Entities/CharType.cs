
using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class CharType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }

    // Navigation Properties
    public ICollection<Character> Characters { get; set; } = new List<Character>();
}
