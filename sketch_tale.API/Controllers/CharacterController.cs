using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.Interfaces.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace sketch_tale.API.Controllers
{
    [Route("api/characters")]
    [ApiController]
    public class CharacterController : ControllerBase
    {
        private readonly ICharacterApprovalService _characterApprovalService;

        public CharacterController(ICharacterApprovalService characterApprovalService)
        {
            _characterApprovalService = characterApprovalService;
        }

        
        [HttpGet("pending-approval")]
        [Authorize(Roles = "parent, Parent")]
        public async Task<IActionResult> GetPendingCharactersForParent()
        {
            try
            {
                var parentId = GetCurrentUserId();
                var list = await _characterApprovalService.GetPendingCharactersForParentAsync(parentId);
                return Ok(list);
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

        [HttpGet("{id}")]
        [Authorize(Roles = "parent, Parent")]
        public async Task<IActionResult> GetCharacterDetail(Guid id)
        {
            try
            {
                var parentId = GetCurrentUserId();
                var detail = await _characterApprovalService.GetCharacterDetailForParentAsync(id, parentId);
                return Ok(detail);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "parent, Parent")]
        public async Task<IActionResult> ApproveCharacter(Guid id)
        {
            try
            {
                var parentId = GetCurrentUserId();
                await _characterApprovalService.ApproveCharacterAsync(id, parentId);
                return Ok(new { message = "Nhân vật đã được phê duyệt thành công!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
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

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "parent, Parent")]
        public async Task<IActionResult> RejectCharacter(Guid id)
        {
            try
            {
                var parentId = GetCurrentUserId();
                await _characterApprovalService.RejectCharacterAsync(id, parentId);
                return Ok(new { message = "Đã từ chối nhân vật thành công!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
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

        [HttpPost("{id}/accept")]
        public async Task<IActionResult> AcceptPreview(Guid id, [FromQuery] Guid childProfileId)
        {
            try
            {
                var result = await _characterApprovalService.AcceptCharacterPreviewAsync(id, childProfileId);
                return Ok(new { message = "Nhân vật đã được gửi đến phụ huynh chờ phê duyệt!", success = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
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

        [HttpPost("{id}/cancel-preview")]
        public async Task<IActionResult> CancelPreview(Guid id, [FromQuery] Guid childProfileId)
        {
            try
            {
                var result = await _characterApprovalService.CancelCharacterPreviewAsync(id, childProfileId);
                return Ok(new { message = "Đã hủy xem trước và hoàn lại 1 lượt tạo nhân vật trong ngày cho bé.", success = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
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

        private Guid GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub")
                         ?? User.FindFirstValue("nameid");

            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

            return userId;
        }
    }
}

