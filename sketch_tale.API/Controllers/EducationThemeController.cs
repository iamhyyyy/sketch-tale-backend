using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Services;

namespace sketch_tale.WebApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class EducationThemeController : ControllerBase
    {
        private readonly IEducationThemeService _educationThemeService;
        private readonly IEmailService _emailService;

        public EducationThemeController(IEducationThemeService educationThemeService, IEmailService emailService)
        {
            _educationThemeService = educationThemeService;
            _emailService = emailService;
        }

        [HttpGet("education-themes")]
        public async Task<ActionResult<List<EducationThemeDto>>> GetAll()
        {
            var items = await _educationThemeService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("education-theme/{id}")]
        public async Task<ActionResult<EducationThemeDto>> GetById(Guid id)
        {
            var item = await _educationThemeService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("education-theme")]
        public async Task<ActionResult<CreateEducationThemeDto>> Create(CreateEducationThemeDto dto)
        {
            var item = await _educationThemeService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPatch("education-theme/{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateEducationThemeDto dto)
        {
            var item = await _educationThemeService.Update(id, dto);
            if (!item) return NotFound();
            return NoContent();
        }

        [HttpGet("test")]
        public async Task<IActionResult> SendTest()
        {
            await _emailService.SendEmailAsync(
                "huyndse184016@fpt.edu.vn",
                "Test Mail",
                "Hello from sketch-tale"
            );

            return Ok("Email sent");
        }
    }
}