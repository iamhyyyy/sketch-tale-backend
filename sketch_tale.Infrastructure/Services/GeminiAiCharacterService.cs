using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Services;
using System.Text;
using System.Text.Json;

namespace sketch_tale.Infrastructure.Services
{
    public class GeminiAiCharacterService : IAiCharacterService
    {
        private readonly HttpClient _httpClient;
        private readonly ICloudinaryService _cloudinary;
        private readonly string _apiKey;
        private readonly string _visionModel;
        private readonly string _imageModel;

        public GeminiAiCharacterService(
            HttpClient httpClient,
            IConfiguration configuration,
            ICloudinaryService cloudinary)
        {
            _httpClient = httpClient;
            _cloudinary = cloudinary;
            _apiKey = configuration["GeminiSettings:ApiKey"] ?? throw new InvalidOperationException(
                "Gemini API Key chưa được cấu hình."
            );
            _visionModel = configuration["GeminiSettings:VisionModel"] ?? "gemini-2.5-flash-lite";
            _imageModel = configuration["GeminiSettings:ImageGenerationModel"] ?? "gemini-2.5-flash-image";
        }

        public async Task<AiAnalyzeResultDto> AnalyzeImageAsync(string imageUrl, IReadOnlyCollection<AiAllowedCharTypeDto> allowedCharTypes)
        {
            var image = await GetBase64FromUrlAsync(imageUrl);
            var charTypesJson = JsonSerializer.Serialize(
                allowedCharTypes.Select(c => new { id = c.Id, nameEn = c.NameEn })
            );

            var analysisPrompt =
                $$"""
                You are an AI classification and visual description assistant for Sketch Tale, a children's drawing application.

                YOUR MAIN TASK:
                1. Analyze the child's drawing to determine if it depicts an identifiable CHARACTER (e.g., Human, Animal, Monster, Mythical Creature, Robot, or Animated Object with living traits).
                2. If a valid character is detected, generate a detailed visual description prompt for image generation models (Midjourney, Flux, Stable Diffusion) and extract descriptive visual tags.
                3. Compare the drawing against the provided ALLOWED CHARACTER TYPES list from the database and suggest up to 3 most related CharType IDs.

                ALLOWED CHARACTER TYPES:
                {{charTypesJson}}

                LOGIC RULES:

                CASE 1: THE DRAWING IS RECOGNIZABLE (isRecognized = true)
                - Describe the drawing clearly, vividly, and objectively (subject, actions, colors, style, key features) to form a prompt for image generation models. Put this in "prompt".
                - Select up to 3 most related CharType IDs from the provided ALLOWED CHARACTER TYPES list (ranked from most relevant to least relevant) and put them in "suggestedCharTypeIds".
                - Extract short descriptive English tags (e.g., colors, facial expression, posture, key features) and put them in "detectedTags".

                CASE 2: THE DRAWING IS UNCLEAR OR UNRECOGNIZABLE (isRecognized = false)
                - Set "prompt" to null.
                - Set "detectedTags" to null.
                - Analyze the visible visual features (shapes, colors, lines, body parts, structures).
                - Based on these visual features, suggest up to 3 closest matching CharType IDs from the provided ALLOWED CHARACTER TYPES list and put them in "suggestedCharTypeIds" (ranked from most relevant to least relevant).

                CRITICAL CONSTRAINTS:
                - Only return exact IDs that exist in ALLOWED CHARACTER TYPES (maximum 3 IDs).
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
                            new
                            {
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
                            suggestedCharTypeIds = new
                            {
                                type = "ARRAY",
                                items = new { type = "STRING" }
                            },
                            detectedTags = new
                            {
                                type = "ARRAY",
                                nullable = true,
                                items = new { type = "STRING" }
                            }
                        },
                        required = new[] { "isRecognized", "prompt", "suggestedCharTypeIds", "detectedTags" }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            // string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash-lite:generateContent";
            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_visionModel}:generateContent";

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl);

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

        public async Task<string> GenerateImageAsync(string finalPrompt, string originalImageUrl)
        {
            var originalImage = await GetBase64FromUrlAsync(originalImageUrl);

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = finalPrompt },
                            new
                            {
                                inlineData = new
                                {
                                    mimeType = originalImage.MimeType,
                                    data = originalImage.Base64
                                }
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    responseModalities = new[] { "TEXT", "IMAGE" }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_imageModel}:generateContent";

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            httpRequest.Headers.Add("x-goog-api-key", _apiKey);
            httpRequest.Content = content;

            using var response = await _httpClient.SendAsync(httpRequest);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                // Nếu tài khoản Gemini đang ở gói Free Tier (bị giới hạn 429/403/404 khi gọi model tạo ảnh),
                // tự động chuyển sang cơ chế Fallback sinh ảnh miễn phí (Pollinations Flux) rồi upload lên Cloudinary
                if (statusCode == 404 || statusCode == 429 || statusCode == 403 || statusCode == 400)
                {
                    return await GenerateImageFallbackAsync(finalPrompt);
                }

                throw new HttpRequestException(
                    $"Gemini Image Generation API lỗi: {statusCode} - {responseString}"
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
                    $"Gemini không trả về ảnh nhân vật. Block reason: {blockReason}"
                );
            }

            var candidate = candidates[0];
            if (!candidate.TryGetProperty("content", out var candidateContent) ||
                !candidateContent.TryGetProperty("parts", out var parts) ||
                parts.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException(
                    "Gemini không trả về dữ liệu content/parts cho ảnh nhân vật."
                );
            }

            string? generatedBase64 = null;
            string generatedMimeType = "image/png";

            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("inlineData", out var inlineData) &&
                    inlineData.ValueKind == JsonValueKind.Object)
                {
                    if (inlineData.TryGetProperty("data", out var dataElement) &&
                        dataElement.ValueKind == JsonValueKind.String)
                    {
                        generatedBase64 = dataElement.GetString();
                    }

                    if (inlineData.TryGetProperty("mimeType", out var mimeElement) &&
                        mimeElement.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrWhiteSpace(mimeElement.GetString()))
                    {
                        generatedMimeType = mimeElement.GetString()!;
                    }

                    if (!string.IsNullOrWhiteSpace(generatedBase64))
                    {
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(generatedBase64))
            {
                throw new InvalidOperationException(
                    "Gemini không trả về dữ liệu hình ảnh (inlineData) trong phản hồi."
                );
            }

            var imageBytes = Convert.FromBase64String(generatedBase64);
            return await UploadBytesToCloudinaryAsync(imageBytes, generatedMimeType);
        }

        private async Task<string> GenerateImageFallbackAsync(string finalPrompt)
        {
            var encodedPrompt = Uri.EscapeDataString(finalPrompt);
            var seed = Random.Shared.Next(1, 999999);
            var fallbackUrl = $"https://image.pollinations.ai/prompt/{encodedPrompt}?width=768&height=768&seed={seed}&nologo=true&model=flux";

            using var fallbackResponse = await _httpClient.GetAsync(fallbackUrl);
            fallbackResponse.EnsureSuccessStatusCode();

            var imageBytes = await fallbackResponse.Content.ReadAsByteArrayAsync();
            if (imageBytes.Length == 0)
            {
                throw new InvalidOperationException("Không thể sinh ảnh nhân vật từ dịch vụ AI.");
            }

            var mimeType = fallbackResponse.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            return await UploadBytesToCloudinaryAsync(imageBytes, mimeType);
        }

        private async Task<string> UploadBytesToCloudinaryAsync(byte[] imageBytes, string mimeType)
        {
            var extension = mimeType.Contains("jpeg", StringComparison.OrdinalIgnoreCase) ? ".jpg"
                : mimeType.Contains("webp", StringComparison.OrdinalIgnoreCase) ? ".webp"
                : ".png";
            var fileName = $"character_{Guid.NewGuid():N}{extension}";

            await using var memoryStream = new MemoryStream(imageBytes);
            var formFile = new FormFile(memoryStream, 0, imageBytes.Length, "File", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = mimeType
            };

            return await _cloudinary.UploadImageAsync(formFile, "CharacterGenerated");
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