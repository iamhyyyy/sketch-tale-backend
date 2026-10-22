using System.ComponentModel.DataAnnotations.Schema;
using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class ChildProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ParentProfileId { get; set; }

    public string NickName { get; set; } = string.Empty;
    public TargetAgeGroup TargetAgeGroup { get; set; }
    public ReadingLevel ReadingLevel { get; set; }

    public int DailyTimeLimit { get; set; } = 30;
    public int RemainingTimeLimit { get; set; } = 30;
    public int DailyCharacterLimit { get; set; } = 1;
    public int RemainingCharacterLimit { get; set; } = 1;

    public ICollection<EducationTheme> AllowedThemes { get; set; } = new List<EducationTheme>();
    public ICollection<StoryTemplate> StoryTemplateRecommends { get; set; } = new List<StoryTemplate>();

    public ICollection<Drawing> Drawings { get; set; } = new List<Drawing>();
    public ICollection<Character> Characters { get; set; } = new List<Character>();
    public ICollection<GeneratedStory> GeneratedStories { get; set; } = new List<GeneratedStory>();

}
