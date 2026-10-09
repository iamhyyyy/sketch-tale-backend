
using sketch_tale.Domain.Common;

namespace sketch_tale.Domain.Entities;

public class ParentStoryRecommendation : BaseEntity
{
    public Guid ChildProfileId { get; set; }
    public List<Guid> StoryTemplateIds { get; set; } = new List<Guid>();


    // Navigation Properties
    public ChildProfile Child { get; set; } = null!;
}
