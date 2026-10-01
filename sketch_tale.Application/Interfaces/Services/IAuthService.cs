using sketch_tale.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Services
{
    public interface IAuthService
    {
        // 1. Phụ huynh tự đăng ký tài khoản (Cần Email)
        Task<bool> RegisterParentAsync(RegisterParentDto model);

        // 2. Phụ huynh đăng nhập (Bằng Email hoặc Username)
        Task<AuthResponseDto?> LoginAsync(LoginDto model);

        // 3. Trẻ em đăng nhập (Chỉ dùng Username, không có email)
        Task<AuthResponseDto?> LoginChildAsync(LoginChildDto model);

        // 4. Phụ huynh tạo tài khoản cho con (Sau khi đăng nhập)
        Task<bool> CreateChildAccountAsync(Guid parentUserId, RegisterChildDto model);

        // 5. Quên mật khẩu (Gửi email token)
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto model);

        // 6. Đặt lại mật khẩu (Bằng token)
        Task<bool> ResetPasswordAsync(ResetPasswordDto model);

        // 7. Đổi mật khẩu (Yêu cầu đăng nhập)
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto model);

        // 8. Xem thông tin cá nhân (Profile)
        Task<UserProfileDto> GetProfileAsync(Guid userId);

        // 9. Xác thực email (Bằng token)
        Task<bool> ConfirmEmailAsync(ConfirmEmailDto model);

        // 10. Gửi lại email xác thực
        Task<bool> ResendConfirmationEmailAsync(ResendConfirmationEmailDto model);

        // 11. Đăng xuất
        Task LogoutAsync();
    }
}
