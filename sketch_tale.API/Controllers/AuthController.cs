using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Services;

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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            var success = await _authService.RegisterAsync(dto);
            if (!success)
            {
                return BadRequest(new { message = "Đăng ký thất bại! Email có thể đã tồn tại." });
            }
            return Ok(new { message = "Đăng ký tài khoản thành công!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (result == null)
            {
                return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác!" });
            }
            return Ok(new { data = result, message = "Đăng nhập thành công!" });
        }
    }
}
