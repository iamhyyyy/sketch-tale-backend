using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "ContentManager,content manager")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<StoryPageTemplateDto>> CreatePage(
        Guid storyId, 
        [FromForm] CreateStoryPageTemplateDto dto, 
        IFormFile? file,
        IFormFile? audioFile)
    {
        try
        {
            var background = file ?? (Request.HasFormContentType ? Request.Form.Files["imageFile"] : null);
            var audio = audioFile ?? (Request.HasFormContentType ? Request.Form.Files["audio"] : null);
            var page = await _storyPageTemplateService.CreatePageAsync(storyId, dto, background, audio);
            return Ok(page);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ContentManager,content manager")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(
        Guid id, 
        [FromForm] UpdateStoryPageTemplateDto dto, 
        IFormFile? file,
        IFormFile? audioFile)
    {
        try
        {
            var background = file ?? (Request.HasFormContentType ? Request.Form.Files["imageFile"] : null);
            var audio = audioFile ?? (Request.HasFormContentType ? Request.Form.Files["audio"] : null);
            var success = await _storyPageTemplateService.UpdatePageAsync(id, dto, background, audio);
            if (!success)
                return NotFound(new { message = $"Trang với Id {id} không tồn tại." });

            return Ok(new { message = "Cập nhật trang thành công." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("story-pages/{id}")]
    [Authorize(Roles = "ContentManager,content manager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _storyPageTemplateService.DeletePageAsync(id);
        if (!success)
            return NotFound(new { message = $"Trang với Id {id} không tồn tại." });

        return NoContent();
    }
}
