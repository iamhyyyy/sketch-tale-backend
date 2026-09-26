using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class ChildDailyUsageLog : BaseEntity
{
    public Guid ChildProfileId { get; set; }

    public DateOnly LogDate { get; set; }
    public int DurationSecond { get; set; }
    public int CharacterCreatedCount { get; set; } = 0;
    public int StoryReadCount { get; set; } = 0;

    public ChildProfile Child { get; set; } = null!;
}
