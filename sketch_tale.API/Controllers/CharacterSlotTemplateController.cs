using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Services;

namespace sketch_tale.WebApi.Controllers;

[ApiController]
[Route("api")]
public class CharacterSlotTemplateController : ControllerBase
{
    private readonly ICharacterSlotTemplateService _characterSlotTemplateService;

    public CharacterSlotTemplateController(ICharacterSlotTemplateService characterSlotTemplateService)
    {
        _characterSlotTemplateService = characterSlotTemplateService;
    }

    [HttpGet("story-pages/{pageId}/slots")]
    public async Task<ActionResult<IEnumerable<CharacterSlotTemplateDto>>> GetSlotsByPage(Guid pageId)
    {
        var slots = await _characterSlotTemplateService.GetSlotsByPageAsync(pageId);
        return Ok(slots);
    }

    [HttpPost("story-pages/{pageId}/slot")]
    public async Task<ActionResult<CharacterSlotTemplateDto>> CreateSlot(
        Guid pageId,
        [FromBody] CreateCharacterSlotTemplateDto dto)
    {
        try
        {
            var slot = await _characterSlotTemplateService.CreateSlotAsync(pageId, dto);
            return Ok(slot);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("story-slots/{id}")]
    public async Task<IActionResult> UpdateSlot(
        Guid id,
        [FromBody] UpdateCharacterSlotTemplateDto dto)
    {
        var success = await _characterSlotTemplateService.UpdateSlotAsync(id, dto);
        if (!success)
            return NotFound(new { message = $"Character slot với Id {id} không tồn tại." });

        return Ok(new { message = "Cập nhật vị trí nhân vật thành công." });
    }

    [HttpDelete("story-slots/{id}")]
    public async Task<IActionResult> DeleteSlot(Guid id)
    {
        var success = await _characterSlotTemplateService.DeleteSlotAsync(id);
        if (!success)
            return NotFound(new { message = $"Character slot với Id {id} không tồn tại." });

        return NoContent();
    }
}
