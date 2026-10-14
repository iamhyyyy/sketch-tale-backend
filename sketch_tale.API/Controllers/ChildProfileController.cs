using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Services;

namespace sketch_tale.API.Controllers
{
    [Route("api/child-profiles")]
    [ApiController]
    public class ChildProfileController : ControllerBase
    {
        private readonly IChildProfileService _childProfileService;

        public ChildProfileController(IChildProfileService childProfileService)
        {
            _childProfileService = childProfileService;
        }
        // GET: api/child-profiles
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var profiles = await _childProfileService.GetAllChildProfilesAsync();
            return Ok(profiles);
        }

        // GET: api/child-profiles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var profile = await _childProfileService.GetByIdAsync(id);
            if (profile == null)
            {
                return NotFound(new { message = "Không tìm thấy hồ sơ bé." });
            }
            return Ok(profile);
        }

        // GET: api/child-profiles/parent/{parentProfileId}
        //[HttpGet("parent/{parentProfileId}")]
        //public async Task<IActionResult> GetByParentId(Guid parentProfileId)
        //{
        //    try
        //    {
        //        var parentProfile = await _childProfileService.GetByParentProfileIdAsync(parentProfileId);
        //        if (parentProfile == null) return NotFound();
        //        return Ok(parentProfile);
        //    }
        //    catch (NotImplementedException)
        //    {
        //        // Bắt lỗi NotImplementedException do bạn đang để throw trong Service
        //        return StatusCode(501, new { message = "Tính năng này chưa được thiết lập." });
        //    }
        //}

        // GET: api/child-profiles/{id}/nickname
        [HttpGet("{id}/nickname")]
        public async Task<IActionResult> GetNickName(Guid id)
        {
            var nickname = await _childProfileService.GetNickNameByIdAsync(id);
            if (nickname == null) return NotFound();

            return Ok(new { NickName = nickname }); // Bọc trong object trả về cho JSON đẹp
        }

        // GET: api/child-profiles/{id}/target-age-group
        [HttpGet("{id}/target-age-group")]
        public async Task<IActionResult> GetTargetAgeGroup(Guid id)
        {
            var ageGroup = await _childProfileService.GetTargetAgeGroupByIdAsync(id);
            if (ageGroup == null) return NotFound();

            return Ok(new { TargetAgeGroup = ageGroup });
        }

        // GET: api/child-profiles/{id}/daily-time-limit
        [HttpGet("{id}/daily-time-limit")]
        public async Task<IActionResult> GetDailyTimeLimit(Guid id)
        {
            var timeLimit = await _childProfileService.GetDailyTimeLimitByIdAsync(id);
            if (timeLimit == null) return NotFound();

            return Ok(new { DailyTimeLimit = timeLimit });
        }

        // GET: api/child-profiles/{id}/daily-character-limit
        [HttpGet("{id}/daily-character-limit")]
        public async Task<IActionResult> GetDailyCharacterLimit(Guid id)
        {
            var charLimit = await _childProfileService.GetDailyCharacterLimitByIdAsync(id);
            if (charLimit == null) return NotFound();

            return Ok(new { DailyCharacterLimit = charLimit });
        }

        // PUT: api/child-profiles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChildProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isUpdated = await _childProfileService.UpdateAsync(id, dto);
            if (!isUpdated)
            {
                return NotFound(new { message = "Không tìm thấy hồ sơ để cập nhật." });
            }
            return Ok(new { message = "Cập nhật thành công!" });
        }
    }
}

