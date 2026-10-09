using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Services;

namespace sketch_tale.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/story-template")]
public class StoryTemplateController : ControllerBase
{
    private readonly IStoryTemplateService _storyTemplateService;

    public StoryTemplateController(IStoryTemplateService storyTemplateService)
    {
        _storyTemplateService = storyTemplateService;
    }

    [HttpGet("getallstories")]
    public async Task<ActionResult<IEnumerable<StoryTemplateDto>>> GetAll()
    {
        var items = await _storyTemplateService.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StoryTemplateDto>> GetById(Guid id)
    {
        var item = await _storyTemplateService.GetByIdAsync(id);
        if (item == null)
            return NotFound(new { message = $"Story template với Id {id} không tồn tại." });

        return Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "content manager")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<StoryTemplateDto>> Create([FromForm] CreateStoryTemplateDto dto, IFormFile? file)
    {
        var item = await _storyTemplateService.CreateAsync(dto, file);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "content manager")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateStoryTemplateDto dto, IFormFile? file)
    {
        var success = await _storyTemplateService.UpdateAsync(id, dto, file);
        if (!success)
            return NotFound(new { message = $"Story template với Id {id} không tồn tại." });

        return Ok(new { message = "Cập nhật story template thành công." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "content manager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _storyTemplateService.DeleteAsync(id);
        if (!success)
            return NotFound(new { message = $"Story template với Id {id} không tồn tại." });

        return NoContent();
    }

    [HttpPost("{id}/publish")]
    [Authorize(Roles = "content manager")]
    public async Task<ActionResult<PublishStoryResultDto>> Publish(Guid id)
    {
        try
        {
            var result = await _storyTemplateService.PublishStoryAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
