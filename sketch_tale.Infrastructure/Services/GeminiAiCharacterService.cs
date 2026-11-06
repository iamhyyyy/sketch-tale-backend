using CloudinaryDotNet.Actions;
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
        

        public GeminiAiCharacterService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AiAnalyzeResult> AnalyzeImageAsync(string imageUrl)
        {
            // 1. Khởi tạo payload gửi lên Gemini API (Ép model trả về JSON bằng generationConfig)
            var payload = new
            {
                contents = new[]
                {
            new
            {
                parts = new object[]
                {
                    // Lệnh prompt đi kèm (System Instruction đã cấu hình sẵn trên AI Studio, 
                    // nên ở đây chỉ cần ra lệnh thực thi phân tích)
                    new { text = "Hãy phân tích hình ảnh này và trả về JSON theo đúng cấu trúc đã quy định." },
                    
                    // Truyền ảnh (Gemini REST API có thể nhận base64 hoặc file URI của Google)
                    // (Lưu ý: Nếu imageUrl là link Cloudinary, bạn có thể cần tải về thành Base64 trước khi gửi 
                    // hoặc dùng Model hỗ trợ đọc URL trực tiếp). Dưới đây là mẫu chuẩn dùng Base64 (inlineData).
                    new {
                        inlineData = new {
                            mimeType = "image/jpeg",
                            data = await GetBase64FromUrlAsync(imageUrl) // Hàm phụ trợ tải ảnh
                        }
                    }
                }
            }
        },
                generationConfig = new
                {
                    responseMimeType = "application/json" // Bắt buộc AI trả về chuẩn JSON
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // 2. Gọi HTTP Post tới Google Gemini
            // (URL này thường lấy từ appsettings.json thông qua IOptions)
            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";
            var response = await _httpClient.PostAsync(apiUrl, content);

            response.EnsureSuccessStatusCode(); // Quăng lỗi nếu gọi API thất bại (4xx, 5xx)

            // 3. Đọc dữ liệu trả về
            var responseString = await response.Content.ReadAsStringAsync();

            // 4. Bóc tách JSON từ response của Gemini 
            // (Vì Gemini bọc JSON trong cấu trúc candidates[0].content.parts[0].text)
            using var document = JsonDocument.Parse(responseString);
            var jsonText = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            // 5. Map chuỗi JSON đó ra object DTO của hệ thống
            var result = JsonSerializer.Deserialize<AiAnalyzeResult>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result ?? new AiAnalyzeResult { IsRecognized = false };
        }

        public Task<AiAnalyzeResult> AnalyzeImageAsync(string imageUrl, List<CharTypeDto> availableCharTypes)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateImageAsync(string finalPrompt, string originalImageUrl)
        {
            throw new NotImplementedException();
        }

        // Hàm phụ trợ tải ảnh từ Cloudinary URL thành Base64 để nhét vào body Gemini
        private async Task<string> GetBase64FromUrlAsync(string url)
        {
            var imageBytes = await _httpClient.GetByteArrayAsync(url);
            return Convert.ToBase64String(imageBytes);
        }
    }
}
