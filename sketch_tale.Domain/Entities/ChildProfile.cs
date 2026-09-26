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

    public int DailyTimeLimit { get; set; } = 30;
    public int DailyCharacterLimit { get; set; } = 1;

    public string? AllowedThemeIdsJson { get; set; }

    public ICollection<Drawing> Drawings { get; set; } = new List<Drawing>();
    public ICollection<Character> Characters { get; set; } = new List<Character>();
    public ICollection<GeneratedStory> GeneratedStories { get; set; } = new List<GeneratedStory>();
}
