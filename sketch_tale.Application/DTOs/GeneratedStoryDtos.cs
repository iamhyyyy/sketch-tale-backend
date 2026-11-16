using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using System;
using System.Collections.Generic;

namespace sketch_tale.Application.DTOs
{
    public class AssignCharacterToRoleDto
    {
        public Guid StoryRoleTemplateId { get; set; }
        public Guid CharacterId { get; set; }
    }

    public class GenerateStoryRequestDto
    {
        public Guid StoryTemplateId { get; set; }
        public Guid? ChildProfileId { get; set; }
       
    }

    public class GeneratedStoryDto : BaseFieldDto
    {
        public Guid ChildProfileId { get; set; }
        public Guid StoryTemplateId { get; set; }
        public int LastPageRead { get; set; }
        public GeneratedStoryStatus Status { get; set; }
        public bool IsFavorite { get; set; }
    }

    public class GeneratedStoryDetailsDto : BaseFieldDto
    {
        public Guid ChildProfileId { get; set; }
        public Guid StoryTemplateId { get; set; }
        public int LastPageRead { get; set; }
        public GeneratedStoryStatus Status { get; set; }
        public bool IsFavorite { get; set; }
        public List<UserStoryCharacterMappingDto> UserStoryCharacterMappings { get; set; } = new();
    }

}
