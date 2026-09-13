using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class CharacterSlotTemplate : BaseEntity
{
    public Guid StoryPageId { get; set; }
    public Guid StoryRoleTemplateId { get; set; }

    public float PosX { get; set; }
    public float PosY { get; set; }
    public float Scale { get; set; } = 1.00f;
    public bool FlipHorizontal { get; set; } = false;
    public CommonStatus Status { get; set; }

    // Navigation Properties
    public StoryPageTemplate StoryPage { get; set; } = null!;
    public StoryRoleTemplate StoryRoleTemplate { get; set; } = null!;
}
