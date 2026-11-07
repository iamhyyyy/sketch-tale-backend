using Microsoft.AspNetCore.Http;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.DTOs
{
    public class AnalyzeDrawingRequest
    {
        public IFormFile File { get; set; } = null!;
        public DrawingType DrawingType { get; set; }
        public Guid ChildProfileId { get; set; }
    }

    public class AiAnalyzeResultDto
    {
        public bool IsRecognized { get; set; }
        public string? Prompt { get; set; }
        public List<string>? DetectedTags { get; set; }
        public List<Guid> SuggestedCharTypeIds { get; set; } = new();
    }

    public class AiAllowedCharTypeDto
    {
        public Guid Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
    }

    public class AnalyzeDrawingResponse
    {
        public Guid DrawingId { get; set; }
        public string OriginalImageUrl { get; set; } = string.Empty;
        public bool IsRecognized { get; set; }
        public List<AiAllowedCharTypeDto> SuggestedCharTypes { get; set; } = new();
        public List<string>? DetectedTags { get; set; }
        public string? StandardizedPrompt { get; set; }
    }

    public class GenerateCharacterRequest
    {
        public Guid DrawingId { get; set; }
        public Guid CharTypeId { get; set; }
        public string? StandardizedPrompt { get; set; }
        public string? ChildAdditionalIdea { get; set; }
        public List<string>? DetectedTags { get; set; }
        public bool IsRegenerate { get; set; }
    }

    public class GenerateCharacterResponse
    {
        public Guid AiCharacterGenerateId { get; set; }
        public string GenerateImageUrl { get; set; } = string.Empty;
        public string AiPromptUsed { get; set; } = string.Empty;
    }
}