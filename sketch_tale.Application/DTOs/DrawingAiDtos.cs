using Microsoft.AspNetCore.Http;
using sketch_tale.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sketch_tale.Application.DTOs
{
    public class NullableGuidConverter : JsonConverter<Guid?>
    {
        public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (string.IsNullOrWhiteSpace(str))
                    return null;

                if (Guid.TryParse(str, out var guid))
                    return guid;

                throw new JsonException($"'{str}' không phải là một GUID hợp lệ.");
            }

            return reader.GetGuid();
        }

        public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }

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
        public string NameVi { get; set; } = string.Empty;
    }

    public class AnalyzeDrawingResponse
    {
        public Guid DrawingId { get; set; }
        public string OriginalImageUrl { get; set; } = string.Empty;
        public bool IsRecognized { get; set; }
        public List<AiAllowedCharTypeDto> SuggestedCharTypes { get; set; } = new();
        public List<string>? DetectedTags { get; set; }
        public string? StandardizedPrompt { get; set; }
        public int? RemainingParentCredits { get; set; }
        public int? RemainingDailyCharacterLimit { get; set; }
    }

    public class GenerateCharacterRequest
    {
        public Guid DrawingId { get; set; }

        [JsonConverter(typeof(NullableGuidConverter))]
        public Guid? CharTypeId { get; set; }

        public string? StandardizedPrompt { get; set; }
        public string? ChildAdditionalIdea { get; set; }
        public List<string>? DetectedTags { get; set; }
        public bool IsRegenerate { get; set; }
    }

    public class GenerateCharacterResponse
    {
        public Guid AiCharacterGenerateId { get; set; }
        public Guid CharacterId { get; set; }
        public Guid DrawingId { get; set; }
        public Guid CharTypeId { get; set; }
        public string GenerateImageUrl { get; set; } = string.Empty;
        public string AiPromptUsed { get; set; } = string.Empty;
        public string AiGenStatus { get; set; } = string.Empty;
        public string CharacterStatus { get; set; } = string.Empty;
        public int? RemainingParentCredits { get; set; }
        public int? RemainingDailyCharacterLimit { get; set; }
    }
}