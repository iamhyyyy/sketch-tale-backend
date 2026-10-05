using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.DTOs
{
    // --- DÀNH CHO PHỤ HUYNH (PARENT) ---

    public class RegisterParentDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; // Phụ huynh bắt buộc phải có Email
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string Identifier { get; set; } = string.Empty; // Có thể nhập Email hoặc Username để đăng nhập
        public string Password { get; set; } = string.Empty;
    }


    // --- DÀNH CHO TRẺ EM (CHILD) ---

    public class RegisterChildDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty; // Tên hiển thị của con
    }

    public class LoginChildDto
    {
        public string Username { get; set; } = string.Empty; // Trẻ em chỉ dùng Username, KHÔNG CÓ EMAIL
        public string Password { get; set; } = string.Empty;
    }


    // --- KẾT QUẢ TRẢ VỀ CHUNG KHI ĐĂNG NHẬP THÀNH CÔNG ---
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; } // Có thể null nếu là Child
        public string Role { get; set; } = string.Empty; // Trả về "Parent", "Child", "Admin", hoặc "ContentManager"
    }

    // --- QUÊN MẬT KHẨU & ĐẶT LẠI MẬT KHẨU ---

    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    // --- XÁC THỰC EMAIL (EMAIL CONFIRMATION) ---

    public class ConfirmEmailDto
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class ResendConfirmationEmailDto
    {
        public string Email { get; set; } = string.Empty;
    }

    // --- ĐỔI MẬT KHẨU (KHI ĐÃ ĐĂNG NHẬP) ---

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    // --- THÔNG TIN HỒ SƠ CÁ NHÂN (VIEW PROFILE) ---

    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? DOB { get; set; }
        public bool? Gender { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public ParentProfileDto? ParentProfile { get; set; }
        public ChildProfileDto? ChildProfile { get; set; }
    }

    public class ParentProfileDto
    {
        public Guid Id { get; set; }
        public int ChildProfileLimit { get; set; }
        public int RemainingChildProfileLimit { get; set; }
        public int MonthlyCreditLimit { get; set; }
        public int RemainingMonthlyCreditLimit { get; set; }
        public bool CanExportStory { get; set; }
        public bool AccessFullStories { get; set; }
    }

    public class ChildProfileDto
    {
        public Guid Id { get; set; }
        public Guid ParentProfileId { get; set; }
        public string NickName { get; set; } = string.Empty;
        public string TargetAgeGroup { get; set; } = string.Empty;
        public int DailyTimeLimit { get; set; }
        public int DailyCharacterLimit { get; set; }
    }
}
