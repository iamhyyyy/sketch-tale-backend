using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Services;

namespace sketch_tale.WebApi.Controllers;

[ApiController]
[Route("api/storypage-template")]
public class StoryPageTemplateController : ControllerBase
{
    private readonly IStoryPageTemplateService _storyPageTemplateService;

    public StoryPageTemplateController(IStoryPageTemplateService storyPageTemplateService)
    {
        _storyPageTemplateService = storyPageTemplateService;
    }

    [HttpGet("pages/{storyId}")]
    public async Task<ActionResult<IEnumerable<StoryPageTemplateDto>>> GetPagesByStory(Guid storyId)
    {
        var pages = await _storyPageTemplateService.GetPagesByStoryAsync(storyId);
        return Ok(pages);
    }

    [HttpPost("{storyId}/page")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<StoryPageTemplateDto>> CreatePage(
        Guid storyId, 
        [FromForm] CreateStoryPageTemplateDto dto, 
        IFormFile? file)
    {
        try
        {
            var page = await _storyPageTemplateService.CreatePageAsync(storyId, dto, file);
            return Ok(page);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(
        Guid id, 
        [FromForm] UpdateStoryPageTemplateDto dto, 
        IFormFile? file)
    {
        var success = await _storyPageTemplateService.UpdatePageAsync(id, dto, file);
        if (!success)
            return NotFound(new { message = $"Trang với Id {id} không tồn tại." });

        return Ok(new { message = "Cập nhật trang thành công." });
    }

    [HttpDelete("story-pages/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _storyPageTemplateService.DeletePageAsync(id);
        if (!success)
            return NotFound(new { message = $"Trang với Id {id} không tồn tại." });

        return NoContent();
    }
}
