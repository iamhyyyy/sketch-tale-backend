using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.DTOs
{
    public class AnalyzeDrawingRequest
    {
        public IFormFile File { get; set; }
        public int DrawingType { get; set; }
    }

    public class AnalyzeDrawingResponse
    {
        public Guid DrawingId { get; set; }
        public string OriginalImageUrl { get; set; }
        public bool IsRecognized { get; set; }
        public List<CharTypeDto> SuggestedCharTypes { get; set; }
        public List<string> DetectedTags { get; set; }
        public string StandardizedPrompt { get; set; }
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
        public string GenerateImageUrl { get; set; }
        public string AiPromptUsed { get; set; }
    }
}
