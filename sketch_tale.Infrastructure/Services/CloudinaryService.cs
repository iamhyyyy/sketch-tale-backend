using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using sketch_tale.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace sketch_tale.Infrastructure.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly Cloudinary _cloudinaryAudio;

        public CloudinaryService(IConfiguration configuration)
        {
            // 1. Cấu hình Cloudinary cho Ảnh
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey    = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true; // dùng HTTPS

            // 2. Cấu hình Cloudinary cho Audio (CloudinaryAudio)
            var audioCloudName = configuration["CloudinaryAudio:CloudName"];
            var audioApiKey    = configuration["CloudinaryAudio:ApiKey"];
            var audioApiSecret = configuration["CloudinaryAudio:ApiSecret"];

            if (!string.IsNullOrWhiteSpace(audioCloudName) &&
                !string.IsNullOrWhiteSpace(audioApiKey) &&
                !string.IsNullOrWhiteSpace(audioApiSecret))
            {
                var audioAccount = new Account(audioCloudName, audioApiKey, audioApiSecret);
                _cloudinaryAudio = new Cloudinary(audioAccount);
                _cloudinaryAudio.Api.Secure = true;
            }
            else
            {
                // Fallback về tài khoản ảnh nếu tài khoản audio chưa cấu hình đầy đủ Key/Secret
                _cloudinaryAudio = _cloudinary;
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ hoặc rỗng.");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File           = new FileDescription(file.FileName, stream),
                Folder         = folder,
                // Tự động resize về 800px chiều rộng, giữ tỉ lệ, tối ưu chất lượng
                Transformation = new Transformation().Width(800).Crop("limit").Quality("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception($"Cloudinary upload lỗi: {result.Error.Message}");

            // SecureUrl là URL HTTPS: https://res.cloudinary.com/...
            return result.SecureUrl.ToString();
        }

        public async Task<string> UploadAudioAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File âm thanh không hợp lệ hoặc rỗng.");

            await using var stream = file.OpenReadStream();

            // Cloudinary xử lý các file Audio (MP3, WAV, AAC, M4A, OGG...) dưới resource Video/Raw
            var uploadParams = new VideoUploadParams
            {
                File         = new FileDescription(file.FileName, stream),
                Folder       = folder,
            };

            var result = await _cloudinaryAudio.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception($"Cloudinary audio upload lỗi: {result.Error.Message}");

            return result.SecureUrl.ToString();
        }

        public async Task DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deleteParams);
        }
    }
}
