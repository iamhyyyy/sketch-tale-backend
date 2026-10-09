using sketch_tale.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.DTOs
{
    //STORY TEMPLATE
    public class CreateStoryTemplateDto : BaseFieldDto
    {
        public Guid EducationThemeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TargetAgeGroup AgeGroup { get; set; }
        public CommonStatus Status { get; set; }
    }
    public class StoryTemplateDto : BaseFieldDto
    {
        public Guid EducationThemeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TargetAgeGroup AgeGroup { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;
        public int TotalPageNumbers { get; set; }
        public CommonStatus Status { get; set; }
    }
    public class UpdateStoryTemplateDto : BaseFieldDto
    {
        public Guid EducationThemeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TargetAgeGroup AgeGroup { get; set; }
        public CommonStatus Status { get; set; }
    }
    //STORY PAGE TEMPLATE
    public class StoryPageTemplateDto : BaseFieldDto
    {
        public Guid StoryTemplateId { get; set; }
        public int PageNumber { get; set; }
        public string RawText { get; set; } = string.Empty;
        public string BackgroundUrl { get; set; } = string.Empty;
        public string? AudioNarrationUrl { get; set; }
        public CommonStatus Status { get; set; }
    }
    public class CreateStoryPageTemplateDto : BaseFieldDto
    {
        public string RawText { get; set; } = string.Empty;
        public string? AudioNarrationUrl { get; set; }
    }
    public class UpdateStoryPageTemplateDto : BaseFieldDto
    {
        public string RawText { get; set; } = string.Empty;
        public string? AudioNarrationUrl { get; set; }
        public CommonStatus Status { get; set; }
    }
    //STORY ROLE TEMPLATE
    public class StoryRoleTemplateDto : BaseFieldDto
    {
        public Guid StoryTemplateId { get; set; }
        public StoryRoleType StoryRoleType { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool AllowCustomCharacter { get; set; }
        public bool RequiresParentApproval { get; set; }
        public string? DefaultAssetUrl { get; set; }
        public CommonStatus Status { get; set; }
    }
    public class CreateStoryRoleTemplateDto : BaseFieldDto
    {
        public StoryRoleType StoryRoleType { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool AllowCustomCharacter { get; set; } = true;
        public bool RequiresParentApproval { get; set; } = false; // Sensitive Role flag
        public string? DefaultAssetUrl { get; set; }
    }
    public class UpdateStoryRoleTemplateDto : BaseFieldDto
    {
        public StoryRoleType StoryRoleType { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool AllowCustomCharacter { get; set; }
        public bool RequiresParentApproval { get; set; }
        public string? DefaultAssetUrl { get; set; }
        public CommonStatus Status { get; set; }
    }
    //CHARACTER SLOT TEMPLATE
    public class CharacterSlotTemplateDto : BaseFieldDto
    {
        public Guid StoryPageId { get; set; }
        public Guid StoryRoleTemplateId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float Scale { get; set; }
        public bool FlipHorizontal { get; set; }
        public CommonStatus Status { get; set; }
    }
    public class CreateCharacterSlotTemplateDto : BaseFieldDto
    {
        public Guid StoryRoleTemplateId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float Scale { get; set; } = 1.00f;
        public bool FlipHorizontal { get; set; } = false;
    }
    //VOCABULARY TEMPLATE
    public class VocabularyTemplateDto : BaseFieldDto
    {
        public Guid StoryPageTemplateId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string? AudioUrl { get; set; }
    }
    public class CreateVocabularyTemplateDto : BaseFieldDto
    {
        public string Word { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string? AudioUrl { get; set; }
    }
    //STORY QUIZ TEMPLATE
    public class StoryQuizTemplateDto : BaseFieldDto
    {
        public Guid StoryTemplateId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string? QuestionAudioUrl { get; set; }
        public string? QuestionPicUrl { get; set; }
        public string OptionsJson { get; set; } = string.Empty;
        public int CorrectOptionIndex { get; set; }
        public CommonStatus Status { get; set; }
    }
    public class CreateStoryQuizTemplateDto : BaseFieldDto
    {
        public string QuestionText { get; set; } = string.Empty;
        public string? QuestionAudioUrl { get; set; }
        public string? QuestionPicUrl { get; set; }
        public List<string> Options { get; set; } = new(); // FE gửi list, BE convert sang JSON
        public int CorrectOptionIndex { get; set; }
    }
    //PUBLISH
    public class PublishStoryResultDto : BaseFieldDto
    {
        public Guid StoryTemplateId { get; set; }
        public string Title { get; set; } = string.Empty;
        public CommonStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }

}
