using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenBlacklistService _tokenBlacklistService;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtService jwtService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEmailService emailService,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            ITokenBlacklistService tokenBlacklistService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _tokenBlacklistService = tokenBlacklistService;
        }

        // 1. PHỤ HUYNH TỰ ĐĂNG KÝ
        public async Task<bool> RegisterParentAsync(RegisterParentDto dto)
        {
            // Check trùng Email qua UserManager
            if (!string.IsNullOrEmpty(dto.Email))
            {
                var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
                if (existingEmail != null)
                {
                    throw new Exception("Email đã tồn tại trong hệ thống.");
                }
            }
            // Check trùng Username qua UserManager
            var existingUsername = await _userManager.FindByNameAsync(dto.Username);

            if (existingUsername != null)   
            {
                throw new Exception("Username đã tồn tại trong hệ thống.");
            }

            // Dùng AutoMapper để chuyển DTO thành User Entity
            var user = _mapper.Map<User>(dto);
            user.CreatedAt = DateTime.UtcNow.AddHours(7);
            user.EmailConfirmed = false; // Phụ huynh bắt buộc phải xác thực email qua link gửi tới hộp thư

            // Tạo user và băm mật khẩu tự động bằng Identity
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = FormatIdentityErrors(result.Errors);
                throw new Exception($"Đăng ký thất bại: {errors}");
            }

            // Mặc định gán Role là "Parent"
            await _userManager.AddToRoleAsync(user, "Parent");

            // Khởi tạo sẵn một ParentProfile đi kèm cho phụ huynh này (Mặc định GÓI 1: FREE - 0k/tháng)
            var parentProfile = CreateDefaultParentProfile(user.Id);

            await _unitOfWork.ParentProfileRepository.AddAsync(parentProfile);
            await _unitOfWork.CompleteAsync();

            // Gửi email xác thực tài khoản
            try
            {
                await SendVerificationEmailAsync(user);
            }
            catch (Exception ex)
            {
                // Nếu gửi email thất bại (do cấu hình email hoặc email không hợp lệ), rollback để user có thể đăng ký lại
                _unitOfWork.ParentProfileRepository.Delete(parentProfile);
                await _unitOfWork.CompleteAsync();
                await _userManager.DeleteAsync(user);
                throw new InvalidOperationException($"Đăng ký thất bại do không thể gửi email xác thực: {ex.Message}");
            }

            return true;
        }

        // 2. ĐĂNG NHẬP CHO 3 ROLE: ADMIN, CONTENT MANAGER, PARENT (Bằng Email hoặc Username)
        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            // Cho phép nhập Email hoặc Username đều được qua UserManager
            var user = await _userManager.FindByEmailAsync(dto.Identifier)
                       ?? await _userManager.FindByNameAsync(dto.Identifier);

            if (user == null)
            {
                return null;
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
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

            // Nếu tài khoản là Child mà cố tình đăng nhập ở cổng này -> Từ chối (không dùng LINQ)
            foreach (var r in roles)
            {
                if (r.Equals("child", StringComparison.OrdinalIgnoreCase))
                {
                    throw new UnauthorizedAccessException("Tài khoản trẻ em không thể đăng nhập tại đây. Vui lòng đăng nhập tại cổng dành cho trẻ em!");
                }
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
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userManager.SetAuthenticationTokenAsync(user, "SketchTale", "RefreshToken", $"{refreshToken}|{refreshTokenExpiry:O}");

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Username = user.UserName ?? string.Empty,
                Email = user.Email,
                Role = matchedRole
            };
        }

        // 3. TRẺ EM ĐĂNG NHẬP (Chỉ dùng Username, không có email)
        public async Task<AuthResponseDto?> LoginChildAsync(LoginChildDto dto)
        {
            // Trẻ em bắt buộc tìm theo Username qua UserManager
            var user = await _userManager.FindByNameAsync(dto.Username);
            if (user == null) return null;

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
            if (!result.Succeeded) return null;

            // Kiểm tra xem user này có đúng là Child không (không dùng LINQ)
            var roles = await _userManager.GetRolesAsync(user);
            bool isChild = false;
            foreach (var r in roles)
            {
                if (r.Equals("child", StringComparison.OrdinalIgnoreCase))
                {
                    isChild = true;
                    break;
                }
            }

            if (!isChild)
            {
                throw new UnauthorizedAccessException("Cổng đăng nhập này chỉ dành riêng cho tài khoản trẻ em!");
            }

            // Tạo token JWT với role "Child"
            var token = _jwtService.GenerateToken(user, "Child");
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userManager.SetAuthenticationTokenAsync(user, "SketchTale", "RefreshToken", $"{refreshToken}|{refreshTokenExpiry:O}");

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Username = user.UserName ?? string.Empty,
                Email = null, // Child không có email
                Role = "Child"
            };
        }

        // 4. PHỤ HUYNH TẠO TÀI KHOẢN CHO CON (CHILD)
        public async Task<bool> CreateChildAccountAsync(Guid parentUserId, RegisterChildDto dto)
        {
            // Lấy ParentProfile qua Repository trực tiếp theo parentUserId (không dùng LINQ)
            var parentProfile = await _unitOfWork.ParentProfileRepository.GetByUserIdAsync(parentUserId);

            // Nếu tài khoản phụ huynh chưa có ParentProfile, tự khởi tạo (mặc định Gói 1: Free)
            if (parentProfile == null)
            {
                parentProfile = CreateDefaultParentProfile(parentUserId);
                await _unitOfWork.ParentProfileRepository.AddAsync(parentProfile);
                await _unitOfWork.CompleteAsync();
            }

            if (parentProfile.RemainingChildProfile <= 0)
            {
                throw new InvalidOperationException($"Phụ huynh đã đạt giới hạn tối đa hồ sơ trẻ em của gói hiện tại ({parentProfile.SubscriptionPlan.ChildProfileLimit} bé)! Vui lòng nâng cấp gói để tiếp tục tạo tài khoản cho con.");
            }

            // Kiểm tra username của bé đã tồn tại chưa qua UserManager
            var existingUser = await _userManager.FindByNameAsync(dto.Username);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Username của bé đã tồn tại trong hệ thống. Vui lòng chọn username khác!");
            }

            // Dùng AutoMapper để tạo User cho con với Email để bằng NULL
            var childUser = _mapper.Map<User>(dto);
            childUser.Email = null;
            childUser.CreatedAt = DateTime.UtcNow.AddHours(7);
            childUser.CreateBy = parentUserId;

            var identityResult = await _userManager.CreateAsync(childUser, dto.Password);
            if (!identityResult.Succeeded)
            {
                var errors = FormatIdentityErrors(identityResult.Errors);
                throw new InvalidOperationException($"Tạo tài khoản cho bé thất bại: {errors}");
            }

            // Gán mặc định Role là "Child"
            await _userManager.AddToRoleAsync(childUser, "Child");

            // Dùng AutoMapper để tạo bản ghi ChildProfile liên kết ngược lại với ParentProfile của phụ huynh
            var childProfile = _mapper.Map<ChildProfile>(dto);
            childProfile.UserId = childUser.Id;
            childProfile.ParentProfileId = parentProfile.Id;
            childProfile.CreatedAt = DateTime.UtcNow.AddHours(7);
            childProfile.CreateBy = parentUserId;

            await _unitOfWork.ChildProfileRepository.AddAsync(childProfile);

            // Trừ đi 1 lượt tạo con của phụ huynh
            parentProfile.RemainingChildProfile -= 1;
            parentProfile.UpdatedAt = DateTime.UtcNow.AddHours(7);
            parentProfile.UpdateBy = parentUserId;

            _unitOfWork.ParentProfileRepository.Update(parentProfile);

            await _unitOfWork.CompleteAsync();
            return true;
        }

        // 5. QUÊN MẬT KHẨU (GỬI EMAIL CHỨA NÚT XÁC NHẬN CHO FRONTEND XỬ LÝ)
        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
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
        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var email = dto.Email?.Trim();
            if (!string.IsNullOrWhiteSpace(email) && email.Contains('%'))
            {
                email = System.Net.WebUtility.UrlDecode(email);
            }

            var user = await _userManager.FindByEmailAsync(email ?? string.Empty);
            if (user == null)
            {
                throw new InvalidOperationException("Không tìm thấy người dùng với email được cung cấp.");
            }

            var token = dto.Token?.Trim() ?? string.Empty;
            // Nếu token được copy trực tiếp từ URL query string (chứa %2B, %2F, v.v.), giải mã URL để lấy raw token
            if (token.Contains('%'))
            {
                token = System.Net.WebUtility.UrlDecode(token);
            }

            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = FormatIdentityErrors(result.Errors);
                throw new InvalidOperationException($"Đặt lại mật khẩu thất bại: {errors}");
            }

            return true;
        }

        // 7. ĐỔI MẬT KHẨU (KHI ĐÃ ĐĂNG NHẬP)
        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new InvalidOperationException("Không tìm thấy thông tin tài khoản người dùng.");
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = FormatIdentityErrors(result.Errors);
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

            // Dùng AutoMapper để chuyển User Entity thành UserProfileDto
            var profileDto = _mapper.Map<UserProfileDto>(user);
            profileDto.Role = primaryRole;

            if (primaryRole == "Parent")
            {
                var parentProfile = await _unitOfWork.ParentProfileRepository.GetByUserIdAsync(user.Id);
                
                if (parentProfile != null)
                {
                    var currentPlan = await _unitOfWork.SubscriptionPlanRepository.GetByIdAsync(parentProfile.SubscriptionPlanId);
                    if (currentPlan != null)
                    {
                        var parentDto = _mapper.Map<ParentProfileDto>(parentProfile);
                        // Xác định gói dịch vụ hiện tại:
                        if (currentPlan.Code == "ProPlan") parentDto.CurrentPlan = "Pro";
                        else if (currentPlan.Code == "PlusPlan") parentDto.CurrentPlan = "Plus";
                        else if (currentPlan.Code == "FreePlan") parentDto.CurrentPlan = "Free";

                        profileDto.ParentProfile = parentDto;
                    }
                }
            }
            else if (primaryRole == "Child")
            {
                var childProfile = await _unitOfWork.ChildProfileRepository.GetByUserIdAsync(user.Id);
                if (childProfile != null)
                {
                    profileDto.ChildProfile = _mapper.Map<ChildProfileDto>(childProfile);
                }
            }

            return profileDto;
        }

        // 9. XÁC THỰC EMAIL (BẰNG TOKEN)
        public async Task<bool> ConfirmEmailAsync(ConfirmEmailDto dto)
        {
            var email = dto.Email?.Trim();
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

            var token = dto.Token?.Trim() ?? string.Empty;
            // Tự động giải mã nếu token được copy từ URL query string
            if (token.Contains('%'))
            {
                token = System.Net.WebUtility.UrlDecode(token);
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                var errors = FormatIdentityErrors(result.Errors);
                throw new InvalidOperationException($"Xác thực email thất bại: {errors}");
            }

            return true;
        }

        // 10. GỬI LẠI EMAIL XÁC THỰC
        public async Task<bool> ResendConfirmationEmailAsync(ResendConfirmationEmailDto dto)
        {
            var email = dto.Email?.Trim();
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

        // 11. ĐĂNG XUẤT (VÔ HIỆU HÓA ACCESS TOKEN NGAY LẬP TỨC VÀ HỦY REFRESH TOKEN)
        public async Task LogoutAsync(string? token = null)
        {
            // 1. Nếu không truyền token cụ thể, tự trích xuất Bearer token từ HttpContext Request Authorization Header
            if (string.IsNullOrEmpty(token))
            {
                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = authHeader.Substring("Bearer ".Length).Trim();
                }
            }

            // 2. Blacklist access token vào memory cache để vô hiệu hóa tức thì
            if (!string.IsNullOrEmpty(token))
            {
                await _tokenBlacklistService.BlacklistTokenAsync(token);
            }

            // 3. Xoá RefreshToken của user trong bảng UserTokens (Identity)
            var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("sub")
                         ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("nameid");

            if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var userId))
            {
                var user = await _userManager.FindByIdAsync(userIdStr);
                if (user != null)
                {
                    await _userManager.RemoveAuthenticationTokenAsync(user, "SketchTale", "RefreshToken");
                }
            }

            await _signInManager.SignOutAsync();
        }

        // 12. CẤP LẠI TOKEN MỚI QUA REFRESH TOKEN (REFRESH TOKEN ROTATION - HỖ TRỢ CẢ AUTHORIZE VÀ BODY)
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto? dto = null, Guid? userId = null)
        {
            string? inputRefreshToken = dto?.RefreshToken?.Trim();

            // 1. Nếu không truyền refreshToken qua body, tự động lấy userId từ HttpContext/Authorize
            if (string.IsNullOrWhiteSpace(inputRefreshToken) && userId == null)
            {
                var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("sub")
                             ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("nameid");

                if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var parsedId))
                {
                    userId = parsedId;
                }
            }

            IdentityUserToken<Guid>? matchedToken = null;
            User? user = null;

            // 2. Nếu có userId (từ Authorize), tìm RefreshToken của user đó trong database
            if (userId != null && userId != Guid.Empty)
            {
                user = await _userManager.FindByIdAsync(userId.Value.ToString());
                if (user != null)
                {
                    var tokenValue = await _userManager.GetAuthenticationTokenAsync(user, "SketchTale", "RefreshToken");
                    if (!string.IsNullOrEmpty(tokenValue))
                    {
                        var parts = tokenValue.Split('|');
                        if (string.IsNullOrWhiteSpace(inputRefreshToken) || parts[0] == inputRefreshToken)
                        {
                            matchedToken = new IdentityUserToken<Guid>
                            {
                                UserId = user.Id,
                                LoginProvider = "SketchTale",
                                Name = "RefreshToken",
                                Value = tokenValue
                            };
                        }
                    }
                }
            }

            // 3. Nếu chưa tìm thấy và client có truyền inputRefreshToken, duyệt DB tìm token tương ứng
            if (matchedToken == null && !string.IsNullOrWhiteSpace(inputRefreshToken))
            {
                var allTokens = await _unitOfWork.Repository<IdentityUserToken<Guid>>().GetAllAsync();
                foreach (var t in allTokens)
                {
                    if (t.LoginProvider == "SketchTale" && t.Name == "RefreshToken")
                    {
                        var parts = (t.Value ?? string.Empty).Split('|');
                        if (parts.Length > 0 && parts[0] == inputRefreshToken)
                        {
                            matchedToken = t;
                            break;
                        }
                    }
                }

                if (matchedToken != null)
                {
                    user = await _userManager.FindByIdAsync(matchedToken.UserId.ToString());
                }
            }

            if (matchedToken == null || user == null)
            {
                throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã bị thu hồi.");
            }

            // 4. Kiểm tra hạn sử dụng của refresh token
            var valParts = (matchedToken.Value ?? string.Empty).Split('|');
            if (valParts.Length >= 2 && DateTime.TryParse(valParts[1], null, System.Globalization.DateTimeStyles.RoundtripKind, out var expiryDate))
            {
                if (DateTime.UtcNow > expiryDate)
                {
                    await _userManager.RemoveAuthenticationTokenAsync(user, "SketchTale", "RefreshToken");
                    throw new UnauthorizedAccessException("Refresh token đã hết hạn. Vui lòng đăng nhập lại.");
                }
            }

            var roles = await _userManager.GetRolesAsync(user);
            string role = "Parent";
            foreach (var r in roles)
            {
                if (r.Equals("admin", StringComparison.OrdinalIgnoreCase)) { role = "Admin"; break; }
                if (r.Equals("content manager", StringComparison.OrdinalIgnoreCase) || r.Equals("contentmanager", StringComparison.OrdinalIgnoreCase)) { role = "ContentManager"; break; }
                if (r.Equals("child", StringComparison.OrdinalIgnoreCase)) { role = "Child"; break; }
                if (r.Equals("parent", StringComparison.OrdinalIgnoreCase)) { role = "Parent"; break; }
            }

            // Tạo access token mới & Refresh token mới (Rotation)
            var newAccessToken = _jwtService.GenerateToken(user, role);
            var newRefreshToken = GenerateRefreshToken();
            var newExpiry = DateTime.UtcNow.AddDays(7);
            await _userManager.SetAuthenticationTokenAsync(user, "SketchTale", "RefreshToken", $"{newRefreshToken}|{newExpiry:O}");

            return new AuthResponseDto
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                Username = user.UserName ?? string.Empty,
                Email = user.Email,
                Role = role
            };
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // 12. LẤY ROLE CỦA USER HIỆN TẠI ĐANG LOGIN (DOUBLE CHECK VỚI DB QUA REPOSITORY, KHÔNG DÙNG LINQ)
        public async Task<string?> GetCurrentUserRoleAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            // 1. Lấy role từ session/token hiện tại của user đang login
            var tokenRole = httpContext.User.FindFirstValue(ClaimTypes.Role)
                         ?? httpContext.User.FindFirstValue("role");

            // Lấy ID của user từ claims (hỗ trợ cả NameIdentifier, sub, nameid)
            var userIdStr = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? httpContext.User.FindFirstValue("sub")
                         ?? httpContext.User.FindFirstValue("nameid");

            if (string.IsNullOrEmpty(userIdStr))
            {
                return tokenRole;
            }

            // 2. Double check lại với role thực tế dưới database bằng UserManager có sẵn (không dùng LINQ)
            var user = await _userManager.FindByIdAsync(userIdStr);
            if (user == null)
            {
                return null;
            }

            var dbRoles = await _userManager.GetRolesAsync(user);
            string? dbRole = null;

            // Duyệt foreach thông thường để chuẩn hóa role theo hệ thống, không dùng LINQ
            foreach (var r in dbRoles)
            {
                if (r.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    dbRole = "Admin";
                    break;
                }
                if (r.Equals("content manager", StringComparison.OrdinalIgnoreCase) || r.Equals("contentmanager", StringComparison.OrdinalIgnoreCase))
                {
                    dbRole = "ContentManager";
                    break;
                }
                if (r.Equals("parent", StringComparison.OrdinalIgnoreCase))
                {
                    dbRole = "Parent";
                    break;
                }
                if (r.Equals("child", StringComparison.OrdinalIgnoreCase))
                {
                    dbRole = "Child";
                    break;
                }
            }

            // Nếu không thuộc 4 role chuẩn trên nhưng trong DB có gán role, lấy role đầu tiên bằng foreach
            if (string.IsNullOrEmpty(dbRole))
            {
                foreach (var r in dbRoles)
                {
                    dbRole = r;
                    break;
                }
            }

            // 3. Double check: so sánh role từ session login với role dưới DB
            // Nếu có role dưới DB:
            // - Nếu không giống nhau (ví dụ: session cũ vẫn giữ role cũ, nhưng admin đã đổi role dưới DB) -> ưu tiên role dưới DB
            // - Nếu giống nhau -> trả về dbRole
            if (!string.IsNullOrEmpty(dbRole))
            {
                return dbRole;
            }

            // Fallback: Nếu DB chưa gán role, giữ role hiện tại từ session token
            return tokenRole;
        }

        public Task<string?> GetCurrentUserRole() => GetCurrentUserRoleAsync();

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

        // HÀM TIỆN ÍCH: KHỞI TẠO HỒ SƠ PHỤ HUYNH VỚI ĐỊNH MỨC GÓI 1: FREE (0K/THÁNG)
        private ParentProfile CreateDefaultParentProfile(Guid userId)
        {
            return new ParentProfile
            {
                UserId = userId,
                RemainingChildProfile = 1,
                RemainingMonthlyCredit = 500,
                TrialPlusPlan = false,
                CreateBy = userId
            };
        }

        // HÀM TIỆN ÍCH: FORMAT LỖI IDENTITY KHÔNG CẦN DÙNG LINQ
        private static string FormatIdentityErrors(IEnumerable<IdentityError> errors)
        {
            var list = new List<string>();
            foreach (var error in errors)
            {
                list.Add(error.Description);
            }
            return string.Join("; ", list);
        }
    }
}