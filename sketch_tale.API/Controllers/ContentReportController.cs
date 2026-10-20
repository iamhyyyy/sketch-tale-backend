using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;

namespace sketch_tale.WebApi.Controllers
{
    [ApiController]
    [Route("api/content-reports")]
    public class ContentReportController : ControllerBase
    {
        private readonly IContentReportService _contentReportService;

        public ContentReportController(IContentReportService ContentReportService)
        {
            _contentReportService = ContentReportService;
        } 

        [HttpGet]
        public async Task<ActionResult<List<ContentReportDto>>> GetAll()
        {
            var items = await _contentReportService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ContentReportDto>> GetById(Guid id)
        {
            var item = await _contentReportService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("users/{userId}")]
        public async Task<ActionResult<List<ContentReportDto>>> GetByUserIdAsync(Guid userId)
        {
            var items = await _contentReportService.GetByParentIdAsync(userId);
            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult<CreateContentReportDto>> Create(CreateContentReportDto dto)
        {
            var item = await _contentReportService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateContentReportDto dto)
        {
            var item = await _contentReportService.Update(id, dto);
            if (!item) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var item = await _contentReportService.Delete(id);
            if (!item) return NotFound();
            return NoContent();
        }
    }
}