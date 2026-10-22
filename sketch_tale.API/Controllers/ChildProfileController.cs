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

        // GET: api/child-profiles/{id}/nickname
        [HttpGet("{id}/nickname")]
        public async Task<IActionResult> GetNickName(Guid id)
        {
            var nickname = await _childProfileService.GetNickNameByIdAsync(id);
            if (nickname == null) return NotFound(new { message = "Không tìm thấy hồ sơ bé." });

            return Ok(new { NickName = nickname });
        }

        // GET: api/child-profiles/{id}/target-age-group
        [HttpGet("{id}/target-age-group")]
        public async Task<IActionResult> GetTargetAgeGroup(Guid id)
        {
            var ageGroup = await _childProfileService.GetTargetAgeGroupByIdAsync(id);
            if (ageGroup == null) return NotFound(new { message = "Không tìm thấy hồ sơ bé." });

            return Ok(new { TargetAgeGroup = ageGroup });
        }

        // GET: api/child-profiles/{id}/daily-time-limit
        [HttpGet("{id}/daily-time-limit")]
        public async Task<IActionResult> GetDailyTimeLimit(Guid id)
        {
            var timeLimit = await _childProfileService.GetDailyTimeLimitByIdAsync(id);
            if (timeLimit == null) return NotFound(new { message = "Không tìm thấy hồ sơ bé." });

            return Ok(new { DailyTimeLimit = timeLimit });
        }

        // GET: api/child-profiles/{id}/daily-character-limit
        [HttpGet("{id}/daily-character-limit")]
        public async Task<IActionResult> GetDailyCharacterLimit(Guid id)
        {
            var charLimit = await _childProfileService.GetDailyCharacterLimitByIdAsync(id);
            if (charLimit == null) return NotFound(new { message = "Không tìm thấy hồ sơ bé." });

            return Ok(new { DailyCharacterLimit = charLimit });
        }

        /// <summary>
        /// Cập nhật thông tin hồ sơ bé (ChildProfile) - Dành cho Phụ huynh và Admin
        /// </summary>
        [Authorize(Roles = "Parent,parent,Admin,admin")]
        // PUT: api/child-profiles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChildProfile(Guid id, [FromBody] UpdateChildProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var isUpdated = await _childProfileService.UpdateChildProfileAsync(id, dto);
                if (!isUpdated)
                {
                    return NotFound(new { message = "Không tìm thấy hồ sơ bé để cập nhật." });
                }

                return Ok(new { message = "Cập nhật hồ sơ bé thành công!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
