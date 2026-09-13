using sketch_tale.Domain.Common;

namespace sketch_tale.Domain.Entities;

public class UserStoryCharacterMapping : BaseEntity
{
    public Guid GeneratedStoryId { get; set; }
    public Guid StoryRoleTemplateId { get; set; }
    public Guid CharacterId { get; set; }

    // Navigation Properties
    public GeneratedStory GeneratedStory { get; set; } = null!;
    public StoryRoleTemplate StoryRoleTemplate { get; set; } = null!;
    public Character Character { get; set; } = null!;
}
