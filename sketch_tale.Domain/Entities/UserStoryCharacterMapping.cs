using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class UserStoryCharacterMapping : BaseEntity
{
    public Guid GeneratedStoryId { get; set; }
    public Guid StoryRoleTemplateId { get; set; }
    public Guid CharacterId { get; set; }

    public ParentApprovalStatus ParentApprovalStatus { get; set; }
    //depend on RequiresParentApproval in table StoryRoleTemplate
    //if false => NotRequired, if true => Pending

    // Navigation Properties
    public GeneratedStory GeneratedStory { get; set; } = null!;
    public StoryRoleTemplate StoryRoleTemplate { get; set; } = null!;
    public Character Character { get; set; } = null!;
}
