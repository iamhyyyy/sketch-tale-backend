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
        Task<bool> RegisterParentAsync(RegisterParentDto dto);

        // 2. Phụ huynh đăng nhập (Bằng Email hoặc Username)
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);

        // 3. Trẻ em đăng nhập (Chỉ dùng Username, không có email)
        Task<AuthResponseDto?> LoginChildAsync(LoginChildDto dto);

        // 4. Phụ huynh tạo tài khoản cho con (Sau khi đăng nhập)
        Task<bool> CreateChildAccountAsync(Guid parentUserId, RegisterChildDto dto);

        // 5. Quên mật khẩu (Gửi email token)
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);

        // 6. Đặt lại mật khẩu (Bằng token)
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);

        // 7. Đổi mật khẩu (Yêu cầu đăng nhập)
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);

        // 8. Xem thông tin cá nhân (Profile)
        Task<UserProfileDto> GetProfileAsync(Guid userId);

        // 9. Xác thực email (Bằng token)
        Task<bool> ConfirmEmailAsync(ConfirmEmailDto dto);

        // 10. Gửi lại email xác thực
        Task<bool> ResendConfirmationEmailAsync(ResendConfirmationEmailDto dto);

        // 11. Đăng xuất (Blacklist access token và huỷ refresh token)
        Task LogoutAsync(string? token = null);

        // 12. Refresh Token (Cấp mới access token và refresh token rotation nhờ Authorize hoặc Body)
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto? dto = null, Guid? userId = null);

        // 13. Lấy role của user hiện tại đang login (double check và ưu tiên theo DB)
        Task<string?> GetCurrentUserRoleAsync();
        //Task<string?> GetCurrentUserRole();

        // 14. Cập nhật role cho người dùng (Chỉ dành cho Admin)
        Task<bool> UpdateRoleAsync(UpdateRoleDto dto);

        // 15. Lấy danh sách tất cả người dùng (Dành cho Admin)
        Task<List<UserProfileDto>> GetAllUsersAsync();

        // 16. Lấy chi tiết người dùng theo ID (Dành cho Admin)
        Task<UserProfileDto> GetUserByIdAsync(Guid userId);

        // 17. Admin chủ động tạo tài khoản (Nhân sự hoặc User mới)
        Task<UserProfileDto> AdminCreateUserAsync(AdminCreateUserDto dto, Guid? adminId);

        // 18. Admin cập nhật thông tin user
        Task<UserProfileDto> AdminUpdateUserAsync(Guid userId, AdminUpdateUserDto dto, Guid? adminId);

        // 19. Khóa hoặc mở khóa tài khoản (Ban/Unban)
        Task<bool> UpdateUserStatusAsync(Guid userId, UpdateUserStatusDto dto, Guid? adminId);
    }
}
