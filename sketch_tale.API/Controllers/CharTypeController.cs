using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;

namespace sketch_tale.WebApi.Controllers
{
    [ApiController]
    [Route("api/char-types")]
    public class CharTypeController : ControllerBase
    {
        private readonly ICharTypeService _charTypeService;

        public CharTypeController(ICharTypeService CharTypeService)
        {
            _charTypeService = CharTypeService;
        } 

        [HttpGet]
        public async Task<ActionResult<List<CharTypeDto>>> GetAll()
        {
            var items = await _charTypeService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CharTypeDto>> GetById(Guid id)
        {
            var item = await _charTypeService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<CreateCharTypeDto>> Create(CreateCharTypeDto dto)
        {
            var item = await _charTypeService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateCharTypeDto dto)
        {
            var item = await _charTypeService.Update(id, dto);
            if (!item) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var item = await _charTypeService.Delete(id);
            if (!item) return NotFound();
            return NoContent();
        }
    }
}