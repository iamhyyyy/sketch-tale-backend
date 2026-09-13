using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class StoryQuizTemplate : BaseEntity
{
    public Guid StoryTemplateId { get; set; }

    public string QuestionText { get; set; } = string.Empty;
    public string? QuestionAudioUrl { get; set; }
    public string? QuestionPicUrl { get; set; }
    public string OptionsJson { get; set; } = string.Empty;
    public int CorrectOptionIndex { get; set; }
    public CommonStatus Status { get; set; }

    // Navigation Properties
    public StoryTemplate StoryTemplate { get; set; } = null!;
}
