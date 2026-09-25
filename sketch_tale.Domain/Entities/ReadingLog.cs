using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class ReadingLog : BaseEntity
{
    public Guid ChildProfileId { get; set; }
    public Guid GeneratedStoryId { get; set; }

    public bool IsCompleted { get; set; }
    public int ReadDurationSeconds { get; set; } = 0;

    // Navigation Properties
    public ChildProfile Child { get; set; } = null!;
    public GeneratedStory GeneratedStory { get; set; } = null!;
    public ICollection<ChildQuizAnswer> ChildQuizAnswers { get; set; } = new List<ChildQuizAnswer>();
}
