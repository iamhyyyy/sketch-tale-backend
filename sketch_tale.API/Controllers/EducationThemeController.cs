using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Services;

namespace sketch_tale.WebApi.Controllers
{
    [ApiController]
    [Route("api/education-themes")]
    public class EducationThemeController : ControllerBase
    {
        private readonly IEducationThemeService _educationThemeService;
        private readonly IEmailService _emailService;

        public EducationThemeController(IEducationThemeService educationThemeService, IEmailService emailService)
        {
            _educationThemeService = educationThemeService;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<ActionResult<List<EducationThemeDto>>> GetAll()
        {
            var items = await _educationThemeService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EducationThemeDto>> GetById(Guid id)
        {
            var item = await _educationThemeService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<CreateEducationThemeDto>> Create(CreateEducationThemeDto dto)
        {
            var item = await _educationThemeService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateEducationThemeDto dto)
        {
            var item = await _educationThemeService.Update(id, dto);
            if (!item) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var item = await _educationThemeService.Delete(id);
            if (!item) return NotFound();
            return NoContent();
        }
    }
}