using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace sketch_tale.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // 1. Phụ huynh tự đăng ký
        [HttpPost("register/parent")]
        public async Task<IActionResult> RegisterParent([FromBody] RegisterParentDto dto)
        {
            try
            {
                var success = await _authService.RegisterParentAsync(dto);
                if (!success)
                {
                    return BadRequest(new { message = "Đăng ký tài khoản thất bại!" });
                }
                return Ok(new { message = "Đăng ký tài khoản phụ huynh thành công! Vui lòng kiểm tra hộp thư email để kích hoạt tài khoản của bạn." });
            }
            catch (Exception ex)
            {
                // Trả về thông báo lỗi chi tiết từ Service (ví dụ: Email đã tồn tại,...)
                return BadRequest(new { message = ex.Message });
            }
        }
        // 2. Phụ huynh tạo tài khoản cho con (Yêu cầu phải đăng nhập tài khoản Parent)
        [HttpPost("register/child")]
        [Authorize(Roles = "Parent,parent")]
        public async Task<IActionResult> RegisterChild([FromBody] RegisterChildDto dto)
        {
            try
            {
                // Lấy ID của phụ huynh từ JWT Token đang đăng nhập (hỗ trợ cả NameIdentifier, sub, nameid)
                var parentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                                ?? User.FindFirstValue("sub")
                                ?? User.FindFirstValue("nameid");

                if (string.IsNullOrEmpty(parentUserId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin định danh phụ huynh từ token." });
                }

                // Gọi service xử lý tạo con và liên kết
                await _authService.CreateChildAccountAsync(Guid.Parse(parentUserId), dto);

                return Ok(new { message = "Tạo tài khoản cho bé thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // 3. Đăng nhập cho 3 role còn lại (Admin, Content Manager, Parent)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                if (result == null)
                {
                    return Unauthorized(new { message = "Email/Username hoặc mật khẩu không chính xác!" });
                }
                return Ok(new { data = result, message = "Đăng nhập thành công!" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 4. Trẻ em đăng nhập (Chỉ dùng Username)
        [HttpPost("login/child")]
        public async Task<IActionResult> LoginChild([FromBody] LoginChildDto dto)
        {
            try
            {
                var result = await _authService.LoginChildAsync(dto);
                if (result == null)
                {
                    return Unauthorized(new { message = "Username hoặc mật khẩu không chính xác!" });
                }
                return Ok(new { data = result, message = "Đăng nhập trẻ em thành công!" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 5. Quên mật khẩu (Gửi email token)
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            try
            {
                await _authService.ForgotPasswordAsync(dto);
                return Ok(new { message = "Nếu email tồn tại trong hệ thống, hướng dẫn đặt lại mật khẩu đã được gửi đến hộp thư của bạn." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 6. Đặt lại mật khẩu (Bằng token đã gửi qua email)
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                await _authService.ResetPasswordAsync(dto);
                return Ok(new { message = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập bằng mật khẩu mới." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 7. Đổi mật khẩu (Yêu cầu đăng nhập)
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin định danh từ token." });
                }

                await _authService.ChangePasswordAsync(userId.Value, dto);
                return Ok(new { message = "Đổi mật khẩu thành công!" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 8. Xem thông tin cá nhân (Profile - Yêu cầu đăng nhập)
        [HttpGet("profile")]
        //[HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin định danh từ token." });
                }

                var profile = await _authService.GetProfileAsync(userId.Value);
                return Ok(new { data = profile, message = "Lấy thông tin tài khoản thành công!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 9. Xác thực email (GET - Hỗ trợ người dùng bấm trực tiếp vào link trên trình duyệt)
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailGet([FromQuery] string email, [FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                {
                    return BadRequest(new { message = "Email và Token xác thực không được để trống." });
                }

                await _authService.ConfirmEmailAsync(new ConfirmEmailDto { Email = email, Token = token });
                return Ok(new { message = "Xác thực email thành công! Tài khoản của bạn đã được kích hoạt. Bạn có thể đăng nhập ngay bây giờ." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 10. Xác thực email (POST - Dành cho Frontend gửi API body hoặc test Swagger)
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmailPost([FromBody] ConfirmEmailDto dto)
        {
            try
            {
                await _authService.ConfirmEmailAsync(dto);
                return Ok(new { message = "Xác thực email thành công! Tài khoản của bạn đã được kích hoạt. Bạn có thể đăng nhập ngay bây giờ." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 11. Gửi lại email xác thực (Trong trường hợp thất lạc hoặc token hết hạn)
        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailDto dto)
        {
            try
            {
                await _authService.ResendConfirmationEmailAsync(dto);
                return Ok(new { message = "Nếu email tồn tại trong hệ thống và chưa được kích hoạt, liên kết xác thực mới đã được gửi vào hộp thư của bạn." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 12. Đăng xuất (Logout)
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _authService.LogoutAsync();
                return Ok(new { message = "Đăng xuất thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                         ?? User.FindFirstValue("sub")
                         ?? User.FindFirstValue("nameid");

            return Guid.TryParse(userIdStr, out var id) ? id : null;
        }
    }
}
