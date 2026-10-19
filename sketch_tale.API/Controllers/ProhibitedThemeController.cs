using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;

namespace sketch_tale.WebApi.Controllers
{
    [ApiController]
    [Route("api/prohibited-themes")]
    public class ProhibitedThemeController : ControllerBase
    {
        private readonly IProhibitedThemeService _prohibitedThemeService;

        public ProhibitedThemeController(IProhibitedThemeService ProhibitedThemeService)
        {
            _prohibitedThemeService = ProhibitedThemeService;
        } 

        [HttpGet]
        public async Task<ActionResult<List<ProhibitedThemeDto>>> GetAll()
        {
            var items = await _prohibitedThemeService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProhibitedThemeDto>> GetById(Guid id)
        {
            var item = await _prohibitedThemeService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<CreateProhibitedThemeDto>> Create(CreateProhibitedThemeDto dto)
        {
            var item = await _prohibitedThemeService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateProhibitedThemeDto dto)
        {
            var item = await _prohibitedThemeService.Update(id, dto);
            if (!item) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var item = await _prohibitedThemeService.Delete(id);
            if (!item) return NotFound();
            return NoContent();
        }
    }
}