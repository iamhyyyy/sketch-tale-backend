using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using sketch_tale.Application.Interfaces.Services;

namespace sketch_tale.Infrastructure.Services
{
    public class ClearBackdropService : IBackgroundRemovalService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ClearBackdropService> _logger;

        public ClearBackdropService(
            HttpClient httpClient, 
            IConfiguration configuration, 
            ILogger<ClearBackdropService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<byte[]> RemoveBackgroundAsync(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return Array.Empty<byte>();

            var apiUrl = _configuration["ClearBackdrop:ApiUrl"]?.Trim();
            if (string.IsNullOrWhiteSpace(apiUrl) ||
                apiUrl.EndsWith("/api/remove", StringComparison.OrdinalIgnoreCase))
            {
                apiUrl = "https://clearbackdrop.com/api/v1/remove-background";
            }

            var apiKey = _configuration["ClearBackdrop:ApiKey"]?.Trim();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("ClearBackdrop ApiKey chưa được cấu hình (hoặc bị ghi đè rỗng trong appsettings.Development.json). Vui lòng kiểm tra ClearBackdrop:ApiKey.");
                return imageBytes; // Fallback ảnh gốc khi chưa có key
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                request.Headers.Add("X-API-Key", apiKey);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                // ClearBackdrop API yêu cầu multipart/form-data với field 'image'
                using var content = new MultipartFormDataContent();
                var (mimeType, ext) = DetectImageFormat(imageBytes);

                var byteContent = new ByteArrayContent(imageBytes);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
                content.Add(byteContent, "image", $"input_image{ext}");

                request.Content = content;

                using var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("ClearBackdrop API lỗi ({StatusCode}) tại {Url}: {Error}. Fallback dùng ảnh gốc.", 
                        (int)response.StatusCode, apiUrl, errorMsg);
                    return imageBytes;
                }

                // ClearBackdrop mặc định trả về thẳng stream binary ảnh PNG trong suốt
                var resultBytes = await response.Content.ReadAsByteArrayAsync();
                if (resultBytes != null && resultBytes.Length > 0)
                {
                    _logger.LogInformation("ClearBackdrop xóa nền thành công ({OriginalSize} bytes -> {ResultSize} bytes).",
                        imageBytes.Length, resultBytes.Length);
                    return resultBytes;
                }

                return imageBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi gọi ClearBackdrop xóa nền. Fallback dùng ảnh gốc.");
                return imageBytes;
            }
        }

        public async Task<byte[]> RemoveBackgroundFromUrlAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return Array.Empty<byte>();

            try
            {
                // Tải ảnh từ URL rồi gửi qua RemoveBackgroundAsync
                using var downloadResponse = await _httpClient.GetAsync(imageUrl);
                downloadResponse.EnsureSuccessStatusCode();
                var imageBytes = await downloadResponse.Content.ReadAsByteArrayAsync();

                return await RemoveBackgroundAsync(imageBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa phông từ URL {Url}. Fallback tải ảnh gốc trả về.", imageUrl);
                using var fallbackDownload = await _httpClient.GetAsync(imageUrl);
                return await fallbackDownload.Content.ReadAsByteArrayAsync();
            }
        }

        private static (string MimeType, string Extension) DetectImageFormat(byte[] bytes)
        {
            if (bytes.Length > 2 && bytes[0] == 0xFF && bytes[1] == 0xD8)
            {
                return ("image/jpeg", ".jpg");
            }
            if (bytes.Length > 3 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            {
                return ("image/png", ".png");
            }
            if (bytes.Length > 11 && bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
            {
                return ("image/webp", ".webp");
            }
            return ("image/png", ".png");
        }
    }
}
