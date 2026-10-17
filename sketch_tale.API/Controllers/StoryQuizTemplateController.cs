using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Services;

namespace sketch_tale.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class StoryQuizTemplateController : ControllerBase
{
    private readonly IStoryQuizTemplateService _storyQuizTemplateService;

    public StoryQuizTemplateController(IStoryQuizTemplateService storyQuizTemplateService)
    {
        _storyQuizTemplateService = storyQuizTemplateService;
    }

    //Này lấy toàn bộ quizz theo câu chuyện(tất cả status)
    [HttpGet("story-template/{storyId}/all-quizzes")]
    [Authorize(Roles = "ContentManager,content manager")]
    public async Task<ActionResult<IEnumerable<StoryQuizTemplateDto>>> GetAllQuizzesByStory(Guid storyId)
    {
        try
        {
            var quizzes = await _storyQuizTemplateService.GetAllQuizzesByStoryAsync(storyId);
            return Ok(quizzes);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    //này lấy quizz theo câu truyện nhưng chỉ cái nào active
    [HttpGet("story-template/{storyId}/quizzes")]
    public async Task<ActionResult<IEnumerable<StoryQuizTemplateDto>>> GetQuizzesByStory(Guid storyId)
    {
        try
        {
            var quizzes = await _storyQuizTemplateService.GetQuizzesByStoryAsync(storyId);
            return Ok(quizzes);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    //lấy quizz theo id
    [HttpGet("story-quizz/{id}")]
    public async Task<ActionResult<StoryQuizTemplateDto>> GetQuizById(Guid id)
    {
        var quiz = await _storyQuizTemplateService.GetQuizByIdAsync(id);
        if (quiz == null)
            return NotFound(new { message = $"Câu hỏi trắc nghiệm với Id {id} không tồn tại." });

        return Ok(quiz);
    }
    //
    //[HttpPost("story-template/{storyId}/quizz")]
    //[Authorize(Roles = "ContentManager,content manager")]
    //public async Task<ActionResult<StoryQuizTemplateDto>> CreateQuiz(
    //    Guid storyId,
    //    [FromBody] CreateStoryQuizTemplateDto dto)
    //{
    //    try
    //    {
    //        var quiz = await _storyQuizTemplateService.CreateQuizAsync(storyId, dto);
    //        return CreatedAtAction(nameof(GetQuizById), new { id = quiz.Id }, quiz);
    //    }
    //    catch (KeyNotFoundException ex)
    //    {
    //        return NotFound(new { message = ex.Message });
    //    }
    //    catch (ArgumentException ex)
    //    {
    //        return BadRequest(new { message = ex.Message });
    //    }
    //}

    [HttpPost("story-template/{storyId}/quizz")]
    [Authorize(Roles = "ContentManager,content manager")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<StoryQuizTemplateDto>> CreateQuizWithImage(
        Guid storyId,
        [FromForm] CreateStoryQuizTemplateDto dto,
        IFormFile? file,
         IFormFile? audioFile)
    {
        try
        {
            var quiz = await _storyQuizTemplateService.CreateQuizAsync(storyId, dto, file, audioFile);
            return CreatedAtAction(nameof(GetQuizById), new { id = quiz.Id }, quiz);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    //[HttpPut("story-quiz/{id}")]
    //[Authorize(Roles = "ContentManager,content manager")]
    //public async Task<IActionResult> UpdateQuiz(
    //    Guid id,
    //    [FromBody] UpdateStoryQuizTemplateDto dto)
    //{
    //    try
    //    {
    //        var success = await _storyQuizTemplateService.UpdateQuizAsync(id, dto);
    //        if (!success)
    //            return NotFound(new { message = $"Câu hỏi trắc nghiệm với Id {id} không tồn tại." });

    //        return Ok(new { message = "Cập nhật câu hỏi trắc nghiệm thành công." });
    //    }
    //    catch (ArgumentException ex)
    //    {
    //        return BadRequest(new { message = ex.Message });
    //    }
    //}

    [HttpPut("story-quiz/{id}/with-image")]
    [Authorize(Roles = "ContentManager,content manager")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateQuizWithImage(
        Guid id,
        [FromForm] UpdateStoryQuizTemplateDto dto,
        IFormFile? file,
         IFormFile? audioFile)
    {
        try
        {
            var success = await _storyQuizTemplateService.UpdateQuizAsync(id, dto, file, audioFile);
            if (!success)
                return NotFound(new { message = $"Câu hỏi trắc nghiệm với Id {id} không tồn tại." });

            return Ok(new { message = "Cập nhật câu hỏi trắc nghiệm thành công." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("story-quiz/{id}")]
    [Authorize(Roles = "ContentManager,content manager")]
    public async Task<IActionResult> DeleteQuiz(Guid id)
    {
        var success = await _storyQuizTemplateService.DeleteQuizAsync(id);
        if (!success)
            return NotFound(new { message = $"Câu hỏi trắc nghiệm với Id {id} không tồn tại." });

        return NoContent();
    }
}
