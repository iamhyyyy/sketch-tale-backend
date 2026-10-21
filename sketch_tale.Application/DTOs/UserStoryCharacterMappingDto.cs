using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.DTOs;

public class UserStoryCharacterMappingDto : BaseFieldDto
{
    public Guid GeneratedStoryId { get; set; }
    public Guid StoryRoleTemplateId { get; set; }
    public Guid CharacterId { get; set; }
    public ParentApprovalStatus ParentApprovalStatus { get; set; }
}

public class CreateUserStoryCharacterMappingDto
{
    public required Guid GeneratedStoryId { get; set; }
    public required Guid StoryRoleTemplateId { get; set; }
    public required Guid CharacterId { get; set; }
}

public class UpdateUserStoryCharacterMappingDto
{
    public Guid GeneratedStoryId { get; set; }
    public Guid StoryRoleTemplateId { get; set; }
    public Guid CharacterId { get; set; }
    public ParentApprovalStatus ParentApprovalStatus { get; set; }
}