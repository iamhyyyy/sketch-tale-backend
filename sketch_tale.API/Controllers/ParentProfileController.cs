using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace sketch_tale.API.Controllers
{
    [Route("api/parent-profiles")] // Đã bỏ dấu [] đi để URL là chuẩn: /api/parent-profiles
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

        // PUT: api/parent-profiles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateParentProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isUpdated = await _parentProfileService.UpdateAsync(id, dto);
            if (!isUpdated)
            {
                return NotFound(new { message = "Không tìm thấy hồ sơ để cập nhật." });
            }

            return Ok(new { message = "Cập nhật thành công!" });
        }

        // PUT: api/parent-profiles/{id}/subscription
        // Dùng FromQuery để nhận các tham số trên URL, hoặc bạn có thể tạo 1 DTO riêng cho body
        //[HttpPut("{id}/subscription")]
        //public async Task<IActionResult> UpdateSubscription(
        //    Guid id,
        //    [FromQuery] int childProfileLimit,
        //    [FromQuery] int monthlyCreditLimit,
        //    [FromQuery] bool canExportStory,
        //    [FromQuery] bool accessFullStories)
        //{
        //    var isUpdated = await _parentProfileService.UpdateProfileForSubscriptionAsync(
        //        id, childProfileLimit, monthlyCreditLimit, canExportStory, accessFullStories);

        //    if (!isUpdated)
        //    {
        //        return NotFound(new { message = "Không tìm thấy hồ sơ phụ huynh để cập nhật gói cước." });
        //    }

        //    return Ok(new { message = "Cập nhật quyền lợi gói Subscription thành công!" });
        //}

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