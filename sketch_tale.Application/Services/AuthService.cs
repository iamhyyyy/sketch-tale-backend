using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtService jwtService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _configuration = configuration;
        }

        // 1. PHỤ HUYNH TỰ ĐĂNG KÝ
        public async Task<bool> RegisterParentAsync(RegisterParentDto model)
        {
            // Check trùng Email an toàn bằng LINQ (tránh lỗi trùng lặp dữ liệu trong DB)
            if (!string.IsNullOrEmpty(model.Email))
            {
                var existingEmail = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingEmail != null)
                {
                    throw new Exception("Email đã tồn tại trong hệ thống.");
                }
            }

            // Check trùng Username
            var existingUsername = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == model.Username);

            if (existingUsername != null)
            {
                throw new Exception("Username đã tồn tại trong hệ thống.");
            }

            // Dùng AutoMapper để chuyển DTO thành User Entity
            var user = _mapper.Map<User>(model);
            user.CreatedAt = DateTime.UtcNow;
            user.EmailConfirmed = false; // Phụ huynh bắt buộc phải xác thực email qua link gửi tới hộp thư

            // Tạo user và băm mật khẩu tự động bằng Identity
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Đăng ký thất bại: {errors}");
            }

            // Mặc định gán Role là "Parent"
            await _userManager.AddToRoleAsync(user, "Parent");

            // Khởi tạo sẵn một ParentProfile đi kèm cho phụ huynh này
            var parentProfile = new ParentProfile
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                CreateBy = user.Id
            };

            await _unitOfWork.Repository<ParentProfile>().AddAsync(parentProfile);
            await _unitOfWork.CompleteAsync();

            // Gửi email xác thực tài khoản
            try
            {
                await SendVerificationEmailAsync(user);
            }
            catch (Exception ex)
            {
                // Nếu gửi email thất bại (do cấu hình email hoặc email không hợp lệ), rollback để user có thể đăng ký lại
                _unitOfWork.Repository<ParentProfile>().Delete(parentProfile);
                await _unitOfWork.CompleteAsync();
                await _userManager.DeleteAsync(user);
                throw new InvalidOperationException($"Đăng ký thất bại do không thể gửi email xác thực: {ex.Message}");
            }

            return true;
        }

        // 2. ĐĂNG NHẬP CHO 3 ROLE: ADMIN, CONTENT MANAGER, PARENT (Bằng Email hoặc Username)
        public async Task<AuthResponseDto?> LoginAsync(LoginDto model)
        {
            // Cho phép nhập Email hoặc Username đều được
            var user = await _userManager.FindByEmailAsync(model.Identifier)
                       ?? await _userManager.FindByNameAsync(model.Identifier);

            if (user == null)
            {
                return null;
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return null;
            }

            // Kiểm tra xác thực email (đối với tài khoản có email như Phụ huynh)
            if (!string.IsNullOrEmpty(user.Email) && !user.EmailConfirmed)
            {
                throw new UnauthorizedAccessException("Tài khoản chưa được xác thực email. Vui lòng kiểm tra hộp thư email của bạn để kích hoạt tài khoản trước khi đăng nhập!");
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Nếu tài khoản là Child mà cố tình đăng nhập ở cổng này -> Từ chối
            if (roles.Any(r => r.Equals("child", StringComparison.OrdinalIgnoreCase)))
            {
                throw new UnauthorizedAccessException("Tài khoản trẻ em không thể đăng nhập tại đây. Vui lòng đăng nhập tại cổng dành cho trẻ em!");
            }

            // Kiểm tra xem user có thuộc một trong các role cho phép: Admin, ContentManager, Parent
            string? matchedRole = null;
            foreach (var r in roles)
            {
                if (r.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    matchedRole = "Admin";
                    break;
                }
                if (r.Equals("content manager", StringComparison.OrdinalIgnoreCase) || r.Equals("contentmanager", StringComparison.OrdinalIgnoreCase))
                {
                    matchedRole = "ContentManager";
                    break;
                }
                if (r.Equals("parent", StringComparison.OrdinalIgnoreCase))
                {
                    matchedRole = "Parent";
                    break;
                }
            }

            if (string.IsNullOrEmpty(matchedRole))
            {
                throw new UnauthorizedAccessException("Tài khoản không có quyền truy cập hệ thống.");
            }

            // Tạo token JWT mang đúng Role thực tế của user
            var token = _jwtService.GenerateToken(user, matchedRole);

            return new AuthResponseDto
            {
                Token = token,
                Username = user.UserName ?? string.Empty,
                Email = user.Email,
                Role = matchedRole
            };
        }

        // 3. TRẺ EM ĐĂNG NHẬP (Chỉ dùng Username, không có email)
        public async Task<AuthResponseDto?> LoginChildAsync(LoginChildDto model)
        {
            // Trẻ em bắt buộc tìm theo Username
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null) return null;

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
            if (!result.Succeeded) return null;

            // Kiểm tra xem user này có đúng là Child không
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any(r => r.Equals("child", StringComparison.OrdinalIgnoreCase)))
            {
                throw new UnauthorizedAccessException("Cổng đăng nhập này chỉ dành riêng cho tài khoản trẻ em!");
            }

            // Tạo token JWT với role "Child"
            var token = _jwtService.GenerateToken(user, "Child");

            return new AuthResponseDto
            {
                Token = token,
                Username = user.UserName ?? string.Empty,
                Email = null, // Child không có email
                Role = "Child"
            };
        }

        // 4. PHỤ HUYNH TẠO TÀI KHOẢN CHO CON (CHILD)
        public async Task<bool> CreateChildAccountAsync(Guid parentUserId, RegisterChildDto model)
        {
            // Lấy danh sách ParentProfile và tìm theo UserId của phụ huynh đang thao tác
            var allParentProfiles = await _unitOfWork.Repository<ParentProfile>().GetAllAsync();
            var parentProfile = allParentProfiles.FirstOrDefault(p => p.UserId == parentUserId);

            // Nếu tài khoản phụ huynh chưa có ParentProfile (ví dụ: tài khoản seed 'parent'), tự khởi tạo
            if (parentProfile == null)
            {
                parentProfile = new ParentProfile
                {
                    UserId = parentUserId,
                    ChildProfileLimit = 3,
                    RemainingChild = 3,
                    CreatedAt = DateTime.UtcNow,
                    CreateBy = parentUserId
                };
                await _unitOfWork.Repository<ParentProfile>().AddAsync(parentProfile);
                await _unitOfWork.CompleteAsync();
            }

            if (parentProfile.RemainingChild <= 0)
            {
                throw new InvalidOperationException("Phụ huynh đã hết lượt tạo tài khoản cho bé! Vui lòng nâng cấp gói dịch vụ.");
            }

            // Kiểm tra username của bé đã tồn tại chưa
            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Username của bé đã tồn tại trong hệ thống. Vui lòng chọn username khác!");
            }

            // Tạo User cho con với Email để bằng NULL
            var childUser = new User
            {
                UserName = model.Username,
                Email = null,
                CreatedAt = DateTime.UtcNow,
                CreateBy = parentUserId
            };

            var identityResult = await _userManager.CreateAsync(childUser, model.Password);
            if (!identityResult.Succeeded)
            {
                var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Tạo tài khoản cho bé thất bại: {errors}");
            }

            // Gán mặc định Role là "Child"
            await _userManager.AddToRoleAsync(childUser, "Child");

            // Tạo bản ghi ChildProfile liên kết ngược lại với ParentProfile của phụ huynh
            var childProfile = new ChildProfile
            {
                UserId = childUser.Id,
                ParentProfileId = parentProfile.Id,
                NickName = model.NickName,
                CreatedAt = DateTime.UtcNow,
                CreateBy = parentUserId
            };

            await _unitOfWork.Repository<ChildProfile>().AddAsync(childProfile);

            // Trừ đi 1 lượt tạo con của phụ huynh
            parentProfile.RemainingChild -= 1;
            parentProfile.UpdatedAt = DateTime.UtcNow;
            parentProfile.UpdateBy = parentUserId;

            _unitOfWork.Repository<ParentProfile>().Update(parentProfile);

            await _unitOfWork.CompleteAsync();
            return true;
        }

        // 5. QUÊN MẬT KHẨU (GỬI EMAIL CHỨA NÚT XÁC NHẬN CHO FRONTEND XỬ LÝ)
        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Bảo mật: Không tiết lộ sự tồn tại của email
                return true;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Tạo đường dẫn xác nhận chuyển tiếp về Frontend
            var resetBaseUrl = _configuration["ClientSettings:ResetPasswordUrl"] ?? "http://localhost:5173/reset-password";
            var encodedEmail = System.Net.WebUtility.UrlEncode(user.Email ?? string.Empty);
            var encodedToken = System.Net.WebUtility.UrlEncode(token);
            var resetLink = $"{resetBaseUrl}?email={encodedEmail}&token={encodedToken}";

            var subject = "✨ SketchTale - Yêu cầu xác nhận đặt lại mật khẩu";
            var body = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Đặt lại mật khẩu SketchTale</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f3f4f6; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif;'>
    <table role='presentation' width='100%' border='0' cellspacing='0' cellpadding='0' style='background-color: #f3f4f6; padding: 40px 10px;'>
        <tr>
            <td align='center'>
                <table role='presentation' width='100%' border='0' cellspacing='0' cellpadding='0' style='max-width: 580px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0, 0, 0, 0.05); border: 1px solid #e5e7eb;'>
                    
                    <!-- Header Banner -->
                    <tr>
                        <td align='center' style='background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 50%, #db2777 100%); padding: 36px 24px; text-align: center;'>
                            <div style='display: inline-block; background-color: rgba(255, 255, 255, 0.2); padding: 6px 16px; border-radius: 30px; margin-bottom: 12px;'>
                                <span style='color: #ffffff; font-size: 12px; font-weight: 700; letter-spacing: 1px; text-transform: uppercase;'>SketchTale Security</span>
                            </div>
                            <h1 style='margin: 0; color: #ffffff; font-size: 28px; font-weight: 800; letter-spacing: -0.5px;'>✨ SketchTale</h1>
                            <p style='margin: 6px 0 0 0; color: rgba(255, 255, 255, 0.9); font-size: 14px;'>Khơi nguồn trí tưởng tượng & truyện kể sáng tạo cho trẻ thơ</p>
                        </td>
                    </tr>

                    <!-- Body Content -->
                    <tr>
                        <td style='padding: 36px 32px;'>
                            <h2 style='margin: 0 0 16px 0; color: #111827; font-size: 20px; font-weight: 700;'>Yêu cầu đặt lại mật khẩu</h2>
                            <p style='margin: 0 0 16px 0; color: #4b5563; font-size: 15px; line-height: 1.6;'>
                                Xin chào <strong style='color: #111827;'>{user.UserName}</strong>,
                            </p>
                            <p style='margin: 0 0 24px 0; color: #4b5563; font-size: 15px; line-height: 1.6;'>
                                Chúng tôi nhận được yêu cầu khôi phục mật khẩu cho tài khoản liên kết với địa chỉ email <strong style='color: #4f46e5;'>{user.Email}</strong>. Để tiếp tục và đặt mật khẩu mới, vui lòng nhấn vào nút xác nhận bên dưới:
                            </p>

                            <!-- CTA Button -->
                            <table role='presentation' width='100%' border='0' cellspacing='0' cellpadding='0' style='margin: 28px 0;'>
                                <tr>
                                    <td align='center'>
                                        <a href='{resetLink}' target='_blank' style='display: inline-block; background: linear-gradient(135deg, #4f46e5 0%, #6366f1 100%); color: #ffffff; text-decoration: none; font-size: 16px; font-weight: 700; padding: 14px 36px; border-radius: 10px; box-shadow: 0 4px 14px rgba(79, 70, 229, 0.4); text-align: center;'>
                                            Xác Nhận Đặt Lại Mật Khẩu &rarr;
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <!-- Important Security Notice Box -->
                            <div style='background-color: #fffbeb; border-left: 4px solid #f59e0b; border-radius: 8px; padding: 16px; margin: 24px 0;'>
                                <p style='margin: 0 0 6px 0; color: #92400e; font-size: 14px; font-weight: 700;'>
                                    🛡️ Lưu ý bảo mật:
                                </p>
                                <ul style='margin: 0; padding-left: 18px; color: #b45309; font-size: 13px; line-height: 1.5;'>
                                    <li>Liên kết này chỉ có hiệu lực một lần và sẽ tự động hết hạn sau <strong>24 giờ</strong>.</li>
                                    <li>Nếu bạn <strong>không</strong> yêu cầu đặt lại mật khẩu, bạn có thể an tâm bỏ qua email này. Mật khẩu hiện tại của bạn vẫn được bảo vệ an toàn.</li>
                                </ul>
                            </div>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f9fafb; padding: 24px 32px; text-align: center; border-top: 1px solid #e5e7eb;'>
                            <p style='margin: 0 0 6px 0; color: #6b7280; font-size: 13px; font-weight: 500;'>
                                🎨 SketchTale — Đồng hành cùng trí tưởng tượng của trẻ thơ
                            </p>
                            <p style='margin: 0 0 8px 0; color: #9ca3af; font-size: 12px;'>
                                Đây là email tự động, vui lòng không phản hồi trực tiếp vào địa chỉ này.
                            </p>
                            <p style='margin: 0; color: #9ca3af; font-size: 12px;'>
                                &copy; 2026 SketchTale Platform. All rights reserved.
                            </p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            try
            {
                await _emailService.SendEmailAsync(user.Email!, subject, body);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi khi gửi email đặt lại mật khẩu: {ex.Message}");
            }

            return true;
        }

        // 6. ĐẶT LẠI MẬT KHẨU (BẰNG TOKEN)
        public async Task<bool> ResetPasswordAsync(ResetPasswordDto model)
        {
            var email = model.Email?.Trim();
            if (!string.IsNullOrWhiteSpace(email) && email.Contains('%'))
            {
                email = System.Net.WebUtility.UrlDecode(email);
            }

            var user = await _userManager.FindByEmailAsync(email ?? string.Empty);
            if (user == null)
            {
                throw new InvalidOperationException("Không tìm thấy người dùng với email được cung cấp.");
            }

            var token = model.Token?.Trim() ?? string.Empty;
            // Nếu token được copy trực tiếp từ URL query string (chứa %2B, %2F, v.v.), giải mã URL để lấy raw token
            if (token.Contains('%'))
            {
                token = System.Net.WebUtility.UrlDecode(token);
            }

            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Đặt lại mật khẩu thất bại: {errors}");
            }

            return true;
        }

        // 7. ĐỔI MẬT KHẨU (KHI ĐÃ ĐĂNG NHẬP)
        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new InvalidOperationException("Không tìm thấy thông tin tài khoản người dùng.");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Đổi mật khẩu không thành công: {errors}");
            }

            return true;
        }

        // 8. XEM THÔNG TIN CÁ NHÂN (VIEW PROFILE)
        public async Task<UserProfileDto> GetProfileAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new KeyNotFoundException("Không tìm thấy thông tin người dùng.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            string primaryRole = "User";
            foreach (var r in roles)
            {
                if (r.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    primaryRole = "Admin";
                    break;
                }
                if (r.Equals("content manager", StringComparison.OrdinalIgnoreCase) || r.Equals("contentmanager", StringComparison.OrdinalIgnoreCase))
                {
                    primaryRole = "ContentManager";
                    break;
                }
                if (r.Equals("parent", StringComparison.OrdinalIgnoreCase))
                {
                    primaryRole = "Parent";
                    break;
                }
                if (r.Equals("child", StringComparison.OrdinalIgnoreCase))
                {
                    primaryRole = "Child";
                    break;
                }
            }

            var profileDto = new UserProfileDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DOB = user.DOB,
                Gender = user.Gender,
                Role = primaryRole,
                CreatedAt = user.CreatedAt
            };

            if (primaryRole == "Parent")
            {
                var parentProfiles = await _unitOfWork.Repository<ParentProfile>().GetAllAsync();
                var parentProfile = parentProfiles.FirstOrDefault(p => p.UserId == user.Id);
                if (parentProfile != null)
                {
                    profileDto.ParentProfile = new ParentProfileDto
                    {
                        Id = parentProfile.Id,
                        ChildProfileLimit = parentProfile.ChildProfileLimit,
                        RemainingChild = parentProfile.RemainingChild,
                        CharacterLimit = parentProfile.CharacterLimit,
                        RemainingCharacters = parentProfile.RemainingCharacters,
                        ExportStoryLimit = parentProfile.ExportStoryLimit,
                        RemainingExport = parentProfile.RemainingExport,
                        AccessFullStories = parentProfile.AccessFullStories
                    };
                }
            }
            else if (primaryRole == "Child")
            {
                var childProfiles = await _unitOfWork.Repository<ChildProfile>().GetAllAsync();
                var childProfile = childProfiles.FirstOrDefault(c => c.UserId == user.Id);
                if (childProfile != null)
                {
                    profileDto.ChildProfile = new ChildProfileDto
                    {
                        Id = childProfile.Id,
                        ParentProfileId = childProfile.ParentProfileId,
                        NickName = childProfile.NickName,
                        TargetAgeGroup = childProfile.TargetAgeGroup.ToString(),
                        DailyTimeLimit = childProfile.DailyTimeLimit,
                        DailyCharacterLimit = childProfile.DailyCharacterLimit
                    };
                }
            }

            return profileDto;
        }

        // 9. XÁC THỰC EMAIL (BẰNG TOKEN)
        public async Task<bool> ConfirmEmailAsync(ConfirmEmailDto model)
        {
            var email = model.Email?.Trim();
            if (!string.IsNullOrWhiteSpace(email) && email.Contains('%'))
            {
                email = System.Net.WebUtility.UrlDecode(email);
            }

            var user = await _userManager.FindByEmailAsync(email ?? string.Empty);
            if (user == null)
            {
                throw new InvalidOperationException("Không tìm thấy người dùng với email được cung cấp.");
            }

            if (user.EmailConfirmed)
            {
                return true; // Đã xác thực trước đó
            }

            var token = model.Token?.Trim() ?? string.Empty;
            // Tự động giải mã nếu token được copy từ URL query string
            if (token.Contains('%'))
            {
                token = System.Net.WebUtility.UrlDecode(token);
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Xác thực email thất bại: {errors}");
            }

            return true;
        }

        // 10. GỬI LẠI EMAIL XÁC THỰC
        public async Task<bool> ResendConfirmationEmailAsync(ResendConfirmationEmailDto model)
        {
            var email = model.Email?.Trim();
            var user = await _userManager.FindByEmailAsync(email ?? string.Empty);
            if (user == null)
            {
                // Bảo mật: Không tiết lộ sự tồn tại của email
                return true;
            }

            if (user.EmailConfirmed)
            {
                throw new InvalidOperationException("Địa chỉ email này đã được xác thực thành công trước đó rồi.");
            }

            await SendVerificationEmailAsync(user);
            return true;
        }

        // 11. ĐĂNG XUẤT
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        // HÀM TIỆN ÍCH: GỬI EMAIL XÁC THỰC TÀI KHOẢN VỚI GIAO DIỆN HTML ĐẸP MẮT
        private async Task SendVerificationEmailAsync(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmBaseUrl = _configuration["ClientSettings:ConfirmEmailUrl"] ?? "http://localhost:5173/confirm-email";
            var encodedEmail = System.Net.WebUtility.UrlEncode(user.Email ?? string.Empty);
            var encodedToken = System.Net.WebUtility.UrlEncode(token);
            var confirmLink = $"{confirmBaseUrl}?email={encodedEmail}&token={encodedToken}";

            var subject = "✨ SketchTale - Xác thực địa chỉ email tài khoản của bạn";
            var body = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Xác thực tài khoản SketchTale</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f3f4f6; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif;'>
    <table role='presentation' width='100%' border='0' cellspacing='0' cellpadding='0' style='background-color: #f3f4f6; padding: 40px 10px;'>
        <tr>
            <td align='center'>
                <table role='presentation' width='100%' border='0' cellspacing='0' cellpadding='0' style='max-width: 580px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0, 0, 0, 0.05); border: 1px solid #e5e7eb;'>
                    
                    <!-- Header Banner -->
                    <tr>
                        <td align='center' style='background: linear-gradient(135deg, #059669 0%, #10b981 50%, #06b6d4 100%); padding: 36px 24px; text-align: center;'>
                            <div style='display: inline-block; background-color: rgba(255, 255, 255, 0.2); padding: 6px 16px; border-radius: 30px; margin-bottom: 12px;'>
                                <span style='color: #ffffff; font-size: 12px; font-weight: 700; letter-spacing: 1px; text-transform: uppercase;'>SketchTale Account Activation</span>
                            </div>
                            <h1 style='margin: 0; color: #ffffff; font-size: 28px; font-weight: 800; letter-spacing: -0.5px;'>✨ SketchTale</h1>
                            <p style='margin: 6px 0 0 0; color: rgba(255, 255, 255, 0.9); font-size: 14px;'>Khơi nguồn trí tưởng tượng & truyện kể sáng tạo cho trẻ thơ</p>
                        </td>
                    </tr>

                    <!-- Body Content -->
                    <tr>
                        <td style='padding: 36px 32px;'>
                            <h2 style='margin: 0 0 16px 0; color: #111827; font-size: 20px; font-weight: 700;'>Chào mừng bạn gia nhập SketchTale! 🎉</h2>
                            <p style='margin: 0 0 16px 0; color: #4b5563; font-size: 15px; line-height: 1.6;'>
                                Xin chào <strong style='color: #111827;'>{user.UserName}</strong>,
                            </p>
                            <p style='margin: 0 0 24px 0; color: #4b5563; font-size: 15px; line-height: 1.6;'>
                                Cảm ơn bạn đã đăng ký tài khoản tại <strong>SketchTale</strong> với địa chỉ email <strong style='color: #059669;'>{user.Email}</strong>. Để bảo vệ an toàn cho tài khoản và kích hoạt đầy đủ quyền lợi, vui lòng nhấn nút xác thực bên dưới:
                            </p>

                            <!-- CTA Button -->
                            <table role='presentation' width='100%' border='0' cellspacing='0' cellpadding='0' style='margin: 28px 0;'>
                                <tr>
                                    <td align='center'>
                                        <a href='{confirmLink}' target='_blank' style='display: inline-block; background: linear-gradient(135deg, #059669 0%, #10b981 100%); color: #ffffff; text-decoration: none; font-size: 16px; font-weight: 700; padding: 14px 36px; border-radius: 10px; box-shadow: 0 4px 14px rgba(16, 185, 129, 0.4); text-align: center;'>
                                            Kích Hoạt Tài Khoản Ngay &rarr;
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <!-- Important Notice Box -->
                            <div style='background-color: #ecfdf5; border-left: 4px solid #10b981; border-radius: 8px; padding: 16px; margin: 24px 0;'>
                                <p style='margin: 0 0 6px 0; color: #065f46; font-size: 14px; font-weight: 700;'>
                                    💡 Vì sao cần xác thực email?
                                </p>
                                <ul style='margin: 0; padding-left: 18px; color: #047857; font-size: 13px; line-height: 1.5;'>
                                    <li>Đảm bảo địa chỉ email thuộc về bạn và có thể nhận thông báo quan trọng.</li>
                                    <li>Hỗ trợ khôi phục tài khoản dễ dàng khi quên mật khẩu.</li>
                                    <li>Bảo vệ an toàn cho hồ sơ của bé yêu trên nền tảng.</li>
                                </ul>
                            </div>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f9fafb; padding: 24px 32px; text-align: center; border-top: 1px solid #e5e7eb;'>
                            <p style='margin: 0 0 6px 0; color: #6b7280; font-size: 13px; font-weight: 500;'>
                                🎨 SketchTale — Khơi nguồn sáng tạo, nuôi dưỡng ước mơ
                            </p>
                            <p style='margin: 0 0 8px 0; color: #9ca3af; font-size: 12px;'>
                                Đây là email tự động, vui lòng không phản hồi trực tiếp vào địa chỉ này.
                            </p>
                            <p style='margin: 0; color: #9ca3af; font-size: 12px;'>
                                &copy; 2026 SketchTale Platform. All rights reserved.
                            </p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            await _emailService.SendEmailAsync(user.Email!, subject, body);
        }
    }
}