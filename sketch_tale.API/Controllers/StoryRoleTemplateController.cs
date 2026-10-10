using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Services;

namespace sketch_tale.WebApi.Controllers;

[ApiController]
[Route("api")]
public class StoryRoleTemplateController : ControllerBase
{
    private readonly IStoryRoleTemplateService _storyRoleTemplateService;

    public StoryRoleTemplateController(IStoryRoleTemplateService storyRoleTemplateService)
    {
        _storyRoleTemplateService = storyRoleTemplateService;
    }


    [HttpGet("story-template/{storyId}/roles")]
    public async Task<ActionResult<IEnumerable<StoryRoleTemplateDto>>> GetRolesByStory(Guid storyId)
    {
        var roles = await _storyRoleTemplateService.GetRolesByStoryAsync(storyId);
        return Ok(roles);
    }


    [HttpPost("story-template/{storyId}/role")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<StoryRoleTemplateDto>> CreateRole(
        Guid storyId,
        [FromForm] CreateStoryRoleTemplateDto dto,
        IFormFile? file)
    {
        try
        {
            var role = await _storyRoleTemplateService.CreateRoleAsync(storyId, dto, file);
            return Ok(role);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("story-role/{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateRole(
        Guid id,
        [FromForm] UpdateStoryRoleTemplateDto dto,
        IFormFile? file)
    {
        var success = await _storyRoleTemplateService.UpdateRoleAsync(id, dto, file);
        if (!success)
            return NotFound(new { message = $"Nhân vật với Id {id} không tồn tại." });

        return Ok(new { message = "Cập nhật nhân vật thành công." });
    }


    [HttpDelete("story-role/{id}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var success = await _storyRoleTemplateService.DeleteRoleAsync(id);
        if (!success)
            return NotFound(new { message = $"Nhân vật với Id {id} không tồn tại." });

        return NoContent();
    }
}
