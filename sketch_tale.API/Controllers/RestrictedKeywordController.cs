using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;

namespace sketch_tale.WebApi.Controllers
{
    [ApiController]
    [Route("api/restricted-keywords")]
    public class RestrictedKeywordController : ControllerBase
    {
        private readonly IRestrictedKeywordService _restrictedKeywordService;

        public RestrictedKeywordController(IRestrictedKeywordService RestrictedKeywordService)
        {
            _restrictedKeywordService = RestrictedKeywordService;
        } 

        [HttpGet]
        public async Task<ActionResult<List<RestrictedKeywordDto>>> GetAll()
        {
            var items = await _restrictedKeywordService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RestrictedKeywordDto>> GetById(Guid id)
        {
            var item = await _restrictedKeywordService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<CreateRestrictedKeywordDto>> Create(CreateRestrictedKeywordDto dto)
        {
            var item = await _restrictedKeywordService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateRestrictedKeywordDto dto)
        {
            var item = await _restrictedKeywordService.Update(id, dto);
            if (!item) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var item = await _restrictedKeywordService.Delete(id);
            if (!item) return NotFound();
            return NoContent();
        }
    }
}