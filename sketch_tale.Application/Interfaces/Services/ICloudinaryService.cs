using Microsoft.AspNetCore.Http;

namespace sketch_tale.Application.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder);
        Task<string> UploadAudioAsync(IFormFile file, string folder);
        Task DeleteImageAsync(string publicId);
    }
}
