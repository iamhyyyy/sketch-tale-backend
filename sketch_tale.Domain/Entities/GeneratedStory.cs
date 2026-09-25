using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class GeneratedStory : BaseEntity
{
    public Guid ChildProfileId { get; set; }
    public Guid StoryTemplateId { get; set; }
    public bool IsFavorite { get; set; }
    public CommonStatus Status { get; set; }
    public int LastPageRead { get; set; }

    // Navigation Properties
    public ChildProfile Child { get; set; } = null!;
    public StoryTemplate StoryTemplate { get; set; } = null!;
    public ICollection<UserStoryCharacterMapping> UserStoryCharacterMappings { get; set; } = new List<UserStoryCharacterMapping>();
}
