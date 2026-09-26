
using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class StoryTemplate : BaseEntity
{
    public Guid EducationThemeId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TargetAgeGroup AgeGroup { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public int TotalPageNumbers { get; set; }
    public CommonStatus Status { get; set; }

    // Navigation Properties
    public EducationTheme EducationTheme { get; set; } = null!;
    public ICollection<StoryRoleTemplate> StoryRoleTemplates { get; set; } = new List<StoryRoleTemplate>();
    public ICollection<StoryPageTemplate> StoryPageTemplates { get; set; } = new List<StoryPageTemplate>();
    public ICollection<StoryQuizTemplate> StoryQuizTemplates { get; set; } = new List<StoryQuizTemplate>();
}
