using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class ChildQuizAnswer : BaseEntity
{
    public Guid ChildId { get; set; }
    public Guid ReadingLogId { get; set; }
    public Guid StoryQuizTemplateId { get; set; }

    public int QuizScore { get; set; } = 0;
    public int SelectedOptionIndex { get; set; }
    public bool IsCorrect { get; set; }
    public int ResponseTimeSeconds { get; set; } = 0;

    // Navigation Properties
    public ChildProfile Child { get; set; } = null!;
    public ReadingLog ReadingLog { get; set; } = null!;
    public StoryQuizTemplate StoryQuizTemplate { get; set; } = null!;
}
