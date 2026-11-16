using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Services
{
    public interface IBackgroundRemovalService
    {
        /// <summary>
        /// Xóa phông nền từ byte[] của hình ảnh, trả về byte[] của ảnh PNG trong suốt
        /// </summary>
        Task<byte[]> RemoveBackgroundAsync(byte[] imageBytes);

        /// <summary>
        /// Xóa phông nền từ URL ảnh công khai, trả về byte[] của ảnh PNG trong suốt
        /// </summary>
        Task<byte[]> RemoveBackgroundFromUrlAsync(string imageUrl);
    }
}
