using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Services;

namespace sketch_tale.WebApi.Controllers;

[ApiController]
[Route("api")]
public class VocabularyTemplateController : ControllerBase
{
    private readonly IVocabularyTemplateService _vocabularyTemplateService;

    public VocabularyTemplateController(IVocabularyTemplateService vocabularyTemplateService)
    {
        _vocabularyTemplateService = vocabularyTemplateService;
    }

    [HttpGet("story-pages/{pageId}/vocabularies")]
    public async Task<ActionResult<IEnumerable<VocabularyTemplateDto>>> GetVocabsByPage(Guid pageId)
    {
        try
        {
            var vocabs = await _vocabularyTemplateService.GetVocabsByPageAsync(pageId);
            return Ok(vocabs);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("story-vocabulary/{id}")]
    public async Task<ActionResult<VocabularyTemplateDto>> GetVocabById(Guid id)
    {
        var vocab = await _vocabularyTemplateService.GetVocabByIdAsync(id);
        if (vocab == null)
            return NotFound(new { message = $"Từ vựng với Id {id} không tồn tại." });

        return Ok(vocab);
    }

    [HttpPost("story-pages/{pageId}/vocabularies")]
    public async Task<ActionResult<VocabularyTemplateDto>> CreateVocab(
        Guid pageId,
        [FromBody] CreateVocabularyTemplateDto dto)
    {
        try
        {
            var vocab = await _vocabularyTemplateService.CreateVocabAsync(pageId, dto);
            return CreatedAtAction(nameof(GetVocabById), new { id = vocab.Id }, vocab);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("story-vocabulary/{id}")]
    public async Task<IActionResult> UpdateVocab(
        Guid id,
        [FromBody] UpdateVocabularyTemplateDto dto)
    {
        try
        {
            var success = await _vocabularyTemplateService.UpdateVocabAsync(id, dto);
            if (!success)
                return NotFound(new { message = $"Từ vựng với Id {id} không tồn tại." });

            return Ok(new { message = "Cập nhật từ vựng thành công." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("story-vocabulary/{id}")]
    public async Task<IActionResult> DeleteVocab(Guid id)
    {
        var success = await _vocabularyTemplateService.DeleteVocabAsync(id);
        if (!success)
            return NotFound(new { message = $"Từ vựng với Id {id} không tồn tại." });

        return NoContent();
    }
}
