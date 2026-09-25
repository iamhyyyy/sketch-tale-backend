using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class ChildVocabularyProgress : BaseEntity
{
    public Guid ChildProfileId { get; set; }
    public Guid VocabularyTemplateId { get; set; }

    public int TotalListenCount { get; set; } = 0;
    public int TotalQuizAttempts { get; set; } = 0;
    public int CorrectAnswersCount { get; set; } = 0;
    public int WrongAnswersCount { get; set; } = 0;

    // Navigation Properties
    public ChildProfile Child { get; set; } = null!;
    public VocabularyTemplate Vocabulary { get; set; } = null!;
}
