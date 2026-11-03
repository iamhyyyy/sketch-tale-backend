using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sketch_tale.API.Controllers;

[ApiController]
[Authorize]
[Route("api/reading-logs")]
public class ReadingLogController : ControllerBase
{
    private readonly IChildQuizAnswerService _childQuizAnswerService;

    public ReadingLogController(IChildQuizAnswerService childQuizAnswerService)
    {
        _childQuizAnswerService = childQuizAnswerService;
    }

    /// <summary>
    /// Bé nộp câu trả lời cho 1 câu hỏi (tự chấm điểm và trả về kết quả ngay để làm hiệu ứng âm thanh/pháo hoa).
    /// </summary>
    [HttpPost("child-quiz-answers")]
    [Authorize(Roles = "child,Child")]
    public async Task<ActionResult<SubmitQuizAnswerResponseDto>> SubmitAnswer([FromBody] CreateChildQuizAnswerDto dto)
    {
        try
        {
            var result = await _childQuizAnswerService.SubmitAnswerAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
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

    /// <summary>
    /// Lấy kết quả quiz của 1 lượt đọc truyện (xem lại lượt đọc này bé đúng bao nhiêu câu, được mấy sao/điểm).
    /// </summary>
    [HttpGet("{readingLogId}/quiz-answers")]
    [Authorize(Roles = "parent,Parent,child,Child")]
    public async Task<ActionResult<ReadingLogQuizSummaryDto>> GetQuizAnswersByReadingLog(Guid readingLogId)
    {
        try
        {
            var summary = await _childQuizAnswerService.GetQuizAnswersByReadingLogIdAsync(readingLogId);
            return Ok(summary);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy toàn bộ lịch sử kết quả quiz của một bé (tất cả phiên đọc truyện).
    /// </summary>
    [HttpGet("child-profiles/{childProfileId}/quiz-answers")]
    [Authorize(Roles = "parent,Parent,child,Child")]
    public async Task<ActionResult<IEnumerable<ReadingLogQuizSummaryDto>>> GetQuizAnswersByChild(Guid childProfileId)
    {
        try
        {
            var result = await _childQuizAnswerService.GetQuizAnswersByChildIdAsync(childProfileId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
