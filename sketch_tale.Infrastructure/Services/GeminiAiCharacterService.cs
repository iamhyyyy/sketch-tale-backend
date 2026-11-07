using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace sketch_tale.Infrastructure.Services
{
    public class GeminiAiCharacterService : IAiCharacterService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiAiCharacterService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeminiSettings:ApiKey"] ?? throw new InvalidOperationException(
                "Gemini API Key chưa được cấu hình."
            );
        }

        public async Task<AiAnalyzeResultDto> AnalyzeImageAsync(string imageUrl, IReadOnlyCollection<AiAllowedCharTypeDto> allowedCharTypes)
        {
            var image = await GetBase64FromUrlAsync(imageUrl);
            var charTypesJson = JsonSerializer.Serialize(allowedCharTypes.
                                                        Select(c => new{id = c.Id,nameEn = c.NameEn}));
            var analysisPrompt =
                $$"""
                You are an AI classification and visual description assistant for Sketch Tale, a children's drawing application.

                YOUR MAIN TASK:
                1. Analyze the child's drawing to determine if it depicts a identifiable CHARACTER (e.g., Human, Animal, Monster, Mythical Creature, Robot, or Animated Object with living traits).
                2. If a valid character is detected, generate a detailed visual description prompt for image generation models (Midjourney, Flux, Stable Diffusion).
                3. Compare the detected character against the provided CharType list from the database.

                ALLOWED CHARACTER TYPES:
                {{charTypesJson}}

                LOGIC RULES:

                CASE 1: THE DRAWING IS RECOGNIZABLE (isRecognized = true)
                - Describe the drawing clearly, vividly, and objectively (subject, actions, colors, style, key features) to form a prompt for image generation models. Put this in "prompt".
                - Find the EXACT matching CharType from the provided ALLOWED CHARACTER TYPES list.
                - Set "suggestedCharTypeId" to the exact matching ID from the list.
                - If NO CharType in the allowed list matches the drawing, set "suggestedCharTypeId" to null. Do NOT invent new IDs or choose an incorrect type.
                - Set "fallbackSuggestedCharTypeIds" to an empty array [].

                CASE 2: THE DRAWING IS UNCLEAR OR UNRECOGNIZABLE (isRecognized = false)
                - Set "prompt" to a message stating the image could not be clearly recognized (e.g., "The drawing is unclear or unidentifiable.").
                - Set "suggestedCharTypeId" to null.
                - Analyze the visible visual features (shapes, colors, lines, body parts, structures).
                - Based on these visual features, suggest up to 3 closest matching CharType IDs from the provided list and put them in "fallbackSuggestedCharTypeIds".
                - Rank the fallback suggestions from most relevant to least relevant.
                - Return fewer than 3 only when fewer relevant CharTypes are available in the list.

                CRITICAL CONSTRAINTS:
                - Only return exact IDs that exist in ALLOWED CHARACTER TYPES.
                - Never make up or hallucinate IDs.
                - Follow the JSON output schema strictly.
                """;
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = analysisPrompt },
                            new{
                                inlineData = new
                                {
                                    mimeType = image.MimeType,
                                    data = image.Base64
                                }
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    responseMimeType = "application/json",
                    responseSchema = new
                    {
                        type = "OBJECT",
                        properties = new
                        {
                            isRecognized = new { type = "BOOLEAN" },
                            prompt = new { type = "STRING", nullable = true },
                            suggestedCharTypeId = new { type = "STRING", nullable = true },
                            fallbackSuggestedCharTypeIds = new
                            {
                                type = "ARRAY",
                                items = new { type = "STRING" }
                            }
                        },
                        required = new[] { "isRecognized", "prompt", "fallbackSuggestedCharTypeIds" }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash-lite:generateContent";

           using var httpRequest = new HttpRequestMessage(HttpMethod.Post,apiUrl);

            httpRequest.Headers.Add("x-goog-api-key", _apiKey);
            httpRequest.Content = content;

           using var response = await _httpClient.SendAsync(httpRequest);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Gemini API lỗi: {(int)response.StatusCode} - {responseString}"
                );
            }
            using var document = JsonDocument.Parse(responseString);

            var root = document.RootElement;

            if (!root.TryGetProperty("candidates", out var candidates) ||
                candidates.ValueKind != JsonValueKind.Array ||
                candidates.GetArrayLength() == 0)
            {
                var blockReason = "Không xác định";

                if (root.TryGetProperty("promptFeedback", out var feedback) &&
                    feedback.TryGetProperty("blockReason", out var reason))
                {
                    blockReason = reason.GetString() ?? blockReason;
                }

                throw new InvalidOperationException(
                    $"Gemini không trả về kết quả. Block reason: {blockReason}"
                );
            }

            var candidate = candidates[0];

            var finishReason = "Không xác định";

            if (candidate.TryGetProperty("finishReason", out var finish))
            {
                finishReason = finish.GetString() ?? finishReason;
            }
            if (finishReason != "STOP")
            {
                throw new InvalidOperationException(
                    $"Gemini không hoàn thành kết quả. FinishReason: {finishReason}"
                );
            }
            if (!candidate.TryGetProperty("content", out var candidateContent) ||
                !candidateContent.TryGetProperty("parts", out var parts) ||
                parts.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException(
                    "Gemini không trả về content hoặc parts hợp lệ."
                );
            }
            var textParts = new List<string>();

            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var textElement) &&
                    textElement.ValueKind == JsonValueKind.String)
                {
                    var text = textElement.GetString();

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        textParts.Add(text);
                    }
                }
            }
            var jsonText = string.Join("\n", textParts);

            if (string.IsNullOrWhiteSpace(jsonText))
            {
                throw new InvalidOperationException(
                    "Gemini trả về nội dung rỗng."
                );
            }
            AiAnalyzeResultDto? result;
            try
            {
                result = JsonSerializer.Deserialize<AiAnalyzeResultDto>(
                    jsonText,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"JSON Gemini trả về không đúng cấu trúc AiAnalyzeResult: {jsonText}",
                    ex
                );
            }
            return result ?? new AiAnalyzeResultDto
            {
                IsRecognized = false
            };
        }

        public Task<string> GenerateImageAsync(string finalPrompt, string originalImageUrl)
        {
            throw new NotImplementedException();
        }

        private async Task<(string Base64, string MimeType)> GetBase64FromUrlAsync(string url)
        {
            using var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var mimeType = response.Content.Headers.ContentType?.MediaType;

            var supportedTypes = new[]{"image/jpeg","image/png","image/webp","image/heic","image/heif"};

            if (string.IsNullOrWhiteSpace(mimeType) ||
                !supportedTypes.Contains(
                    mimeType,
                    StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Định dạng ảnh không được hỗ trợ: {mimeType}"
                );
            }

            var imageBytes = await response.Content.ReadAsByteArrayAsync();

            if (imageBytes.Length == 0)
            {
                throw new InvalidOperationException("Ảnh tải về bị rỗng.");
            }

            return (
                Convert.ToBase64String(imageBytes),
                mimeType
            );
        }
    }
}
