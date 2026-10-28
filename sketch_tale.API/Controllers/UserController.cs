using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace sketch_tale.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize(Roles = "Admin,admin")]
    public class UserController : ControllerBase
    {
        private readonly IAuthService _authService;

        public UserController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: /api/users - Lấy danh sách toàn bộ người dùng
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();
                return Ok(new { data = users, message = "Lấy danh sách người dùng thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: /api/users/{id} - Lấy chi tiết thông tin 1 người dùng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            try
            {
                var user = await _authService.GetUserByIdAsync(id);
                return Ok(new { data = user, message = "Lấy thông tin người dùng thành công!" });
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

        // POST: /api/users - Admin chủ động tạo tài khoản cho nhân sự hoặc user mới
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var adminId = GetCurrentUserId();
                var createdUser = await _authService.AdminCreateUserAsync(dto, adminId);
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, new
                {
                    data = createdUser,
                    message = "Tạo tài khoản người dùng mới thành công!"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        // PUT: /api/users/{id} - Admin cập nhật thông tin của user
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] AdminUpdateUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var adminId = GetCurrentUserId();
                var updatedUser = await _authService.AdminUpdateUserAsync(id, dto, adminId);
                return Ok(new
                {
                    data = updatedUser,
                    message = "Cập nhật thông tin người dùng thành công!"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        // PATCH: /api/users/{id}/status - Khóa hoặc mở khóa tài khoản (Ban/Unban)
        [HttpPatch("{id}/status")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var adminId = GetCurrentUserId();
                var success = await _authService.UpdateUserStatusAsync(id, dto, adminId);
                if (!success)
                {
                    return BadRequest(new { message = "Cập nhật trạng thái tài khoản thất bại!" });
                }

                var statusAction = dto.IsLocked ? "Khóa (Ban)" : "Mở khóa (Unban)";
                return Ok(new
                {
                    isLocked = dto.IsLocked,
                    message = $"{statusAction} tài khoản thành công!"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi: {ex.Message}" });
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
