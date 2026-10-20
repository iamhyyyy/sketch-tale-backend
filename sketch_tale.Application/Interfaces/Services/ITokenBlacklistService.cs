using System;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Services
{
    public interface ITokenBlacklistService
    {
        /// <summary>
        /// Đưa token vào blacklist để vô hiệu hóa ngay lập tức.
        /// </summary>
        /// <param name="token">JWT Token cần thu hồi</param>
        Task BlacklistTokenAsync(string token);

        /// <summary>
        /// Kiểm tra xem token hoặc jti đã bị vô hiệu hóa (logout) hay chưa.
        /// </summary>
        /// <param name="token">JWT Token hoặc JTI</param>
        /// <returns>True nếu đã bị vô hiệu hóa</returns>
        Task<bool> IsTokenBlacklistedAsync(string token);
    }
}
