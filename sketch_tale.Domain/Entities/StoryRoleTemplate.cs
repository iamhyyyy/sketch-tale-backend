using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class StoryRoleTemplate : BaseEntity
{
    public Guid StoryTemplateId { get; set; }

    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool AllowCustomCharacter { get; set; } = true;
    public bool RequiresParentApproval { get; set; } = false;
    public string? DefaultAssetUrl { get; set; }
    public CommonStatus Status { get; set; }

    // Navigation Properties
    public StoryTemplate StoryTemplate { get; set; } = null!;
}
