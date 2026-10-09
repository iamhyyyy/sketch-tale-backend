using Microsoft.AspNetCore.Http;

namespace sketch_tale.Application.Interfaces
{
    public interface ICloudinaryService
    {
        /// <summary>
        /// Upload một file ảnh lên Cloudinary và trả về URL public.
        /// </summary>
        /// <param name="file">File ảnh từ form (IFormFile)</param>
        /// <param name="folder">Thư mục trên Cloudinary, ví dụ: "pets", "items", "avatars"</param>
        /// <returns>URL của ảnh đã upload</returns>
        Task<string> UploadImageAsync(IFormFile file, string folder);

        /// <summary>
        /// Xóa ảnh trên Cloudinary theo publicId.
        /// </summary>
        Task DeleteImageAsync(string publicId);
    }
}
