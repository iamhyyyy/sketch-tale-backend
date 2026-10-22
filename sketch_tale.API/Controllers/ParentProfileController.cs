using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sketch_tale.API.Controllers
{
    [Route("api/parent-profiles")]
    [ApiController]
    public class ParentProfileController : ControllerBase
    {
        private readonly IParentProfileService _parentProfileService;

        public ParentProfileController(IParentProfileService parentProfileService)
        {
            _parentProfileService = parentProfileService;
        }

        // GET: api/parent-profiles
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var profiles = await _parentProfileService.GetAllAsync();
            return Ok(profiles);
        }

        // GET: api/parent-profiles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var profile = await _parentProfileService.GetByIdAsync(id);
            if (profile == null)
            {
                return NotFound(new { message = "Không tìm thấy hồ sơ phụ huynh." });
            }
            return Ok(profile);
        }

        /// <summary>
        /// Cập nhật gói dịch vụ (Subscription Plan) cho tài khoản phụ huynh - Chỉ dành cho Admin
        /// </summary>
        //[Authorize(Roles = "admin,Admin")]
        // PUT: api/parent-profiles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateParentProfile(Guid id, [FromBody] UpdateParentProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var isUpdated = await _parentProfileService.UpdateAsync(id, dto);
                if (!isUpdated)
                {
                    return NotFound(new { message = "Không tìm thấy hồ sơ phụ huynh để cập nhật." });
                }

                return Ok(new { message = "Cập nhật gói dịch vụ cho hồ sơ phụ huynh thành công!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PATCH (hoặc PUT): api/parent-profiles/{id}/mark-trial-plan-used
        [HttpPatch("{id}/mark-trial-plan-used")]
        public async Task<IActionResult> MarkTrialPlanUsed(Guid id)
        {
            var isUpdated = await _parentProfileService.MarkTrialPlanUsedAsync(id);
            if (!isUpdated)
            {
                return NotFound(new { message = "Không tìm thấy hồ sơ phụ huynh." });
            }

            return Ok(new { message = "Đã đánh dấu tài khoản sử dụng gói dùng thử (Trial)." });
        }
    }
}
