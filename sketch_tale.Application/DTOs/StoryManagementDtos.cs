using sketch_tale.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.DTOs
{
    //STORY TEMPLATE
    public class CreateStoryTemplateDto
    {
        public Guid EducationThemeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TargetAgeGroup AgeGroup { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreateBy { get; set; }
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
    public class UpdateStoryTemplateDto
    {
        public Guid EducationThemeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TargetAgeGroup AgeGroup { get; set; }
        public CommonStatus Status { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdateBy { get; set; }
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
    public class CreateStoryPageTemplateDto
    {
        public string RawText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid CreateBy { get; set; }
    }
    public class UpdateStoryPageTemplateDto
    {
        public string RawText { get; set; } = string.Empty;

        public CommonStatus Status { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdateBy { get; set; }
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
    public class CreateStoryRoleTemplateDto
    {
        public StoryRoleType StoryRoleType { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool AllowCustomCharacter { get; set; } = true;
        public bool RequiresParentApproval { get; set; } = false; // Sensitive Role flag
        public string? DefaultAssetUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreateBy { get; set; }
    }
    public class UpdateStoryRoleTemplateDto
    {
        public StoryRoleType StoryRoleType { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool AllowCustomCharacter { get; set; }
        public bool RequiresParentApproval { get; set; }
        public string? DefaultAssetUrl { get; set; }
        public CommonStatus Status { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdateBy { get; set; }
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
    public class CreateCharacterSlotTemplateDto
    {
        public Guid StoryRoleTemplateId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float Scale { get; set; } = 1.00f;
        public bool FlipHorizontal { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public Guid CreateBy { get; set; }
    }
    public class UpdateCharacterSlotTemplateDto
    {
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float Scale { get; set; } = 1.00f;
        public bool FlipHorizontal { get; set; } = false;
        public CommonStatus Status { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdateBy { get; set; }
    }
    //VOCABULARY TEMPLATE
    public class VocabularyTemplateDto : BaseFieldDto
    {
        public Guid StoryPageTemplateId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string? AudioUrl { get; set; }
    }
    public class CreateVocabularyTemplateDto
    {
        public string Word { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string? AudioUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreateBy { get; set; }
    }
    public class UpdateVocabularyTemplateDto
    {
        public string Word { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string? AudioUrl { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdateBy { get; set; }
    }
    //STORY QUIZ TEMPLATE
    public class StoryQuizTemplateDto : BaseFieldDto
    {
        public Guid StoryTemplateId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string? QuestionAudioUrl { get; set; }
        public string? QuestionPicUrl { get; set; }
        public string OptionsJson { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public int CorrectOptionIndex { get; set; }
        public CommonStatus Status { get; set; }
    }
    public class CreateStoryQuizTemplateDto
    {
        public string QuestionText { get; set; } = string.Empty;

        public List<string> Options { get; set; } = new(); // FE gửi list, BE convert sang JSON
        public int CorrectOptionIndex { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreateBy { get; set; }
    }
    public class UpdateStoryQuizTemplateDto
    {
        public string QuestionText { get; set; } = string.Empty;

        public List<string> Options { get; set; } = new();
        public int CorrectOptionIndex { get; set; }
        public CommonStatus Status { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdateBy { get; set; }
    }
    //PUBLISH
    public class PublishStoryResultDto
    {
        public Guid StoryTemplateId { get; set; }
        public string Title { get; set; } = string.Empty;
        public CommonStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }

}
