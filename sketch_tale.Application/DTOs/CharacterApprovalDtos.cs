using sketch_tale.Domain.Enums;
using System;

namespace sketch_tale.Application.DTOs
{
    public class CharacterApprovalDto
    {
        public Guid CharacterId { get; set; }
        public Guid ChildProfileId { get; set; }
        public string ChildNickName { get; set; } = string.Empty;
        public string CharacterName { get; set; } = string.Empty;
        public string DefaultPronoun { get; set; } = string.Empty;
        public string FinalImageUrl { get; set; } = string.Empty;      // Ảnh do AI sinh ra
        public string OriginalDrawingUrl { get; set; } = string.Empty; // Ảnh do bé tự vẽ hoặc upload
        public string? AiPromptUsed { get; set; }
        public CharacterStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

