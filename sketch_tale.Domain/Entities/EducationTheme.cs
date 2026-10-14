using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class EducationTheme : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public CommonStatus Status { get; set; }

    // Navigation Properties
    public ICollection<StoryTemplate> StoryTemplates { get; set; } = new List<StoryTemplate>();
}
