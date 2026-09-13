using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }

    // Navigation Properties
    public ICollection<StoryTemplate> StoryTemplates { get; set; } = new List<StoryTemplate>();
}
