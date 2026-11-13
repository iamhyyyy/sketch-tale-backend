using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    private readonly IReadingLogService _readingLogService;

    public ReadingLogController(IReadingLogService readingLogService)
    {
        _readingLogService = readingLogService;
    }

    /// <summary>
    /// Bắt đầu phiên đọc một cuốn truyện.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "child,Child")]
    public async Task<ActionResult<ReadingLogDto>> StartReadingSession([FromBody] StartReadingLogDto dto)
    {
        try
        {
            var result = await _readingLogService.StartReadingSessionAsync(dto);
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
    /// Cập nhật tiến độ đọc (trang hiện tại & thời gian).
    /// </summary>
    [HttpPut("{id}/progress")]
    [Authorize(Roles = "child,Child")]
    public async Task<ActionResult<ReadingLogDto>> UpdateReadingProgress(Guid id, [FromBody] UpdateReadingProgressDto dto)
    {
        try
        {
            var result = await _readingLogService.UpdateReadingProgressAsync(id, dto);
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
    /// Đánh dấu bé đã hoàn thành cuốn truyện.
    /// </summary>
    [HttpPut("{id}/complete")]
    [Authorize(Roles = "child,Child")]
    public async Task<ActionResult<ReadingLogDto>> CompleteReadingSession(Guid id)
    {
        try
        {
            var result = await _readingLogService.CompleteReadingSessionAsync(id);
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

    /// <summary>
    /// Lấy danh sách các truyện bé đã đọc xong (để hiển thị ở Góc của bé & Trang chủ).
    /// </summary>
    [HttpGet("child-profiles/{childProfileId}/completed")]
    [Authorize(Roles = "parent,Parent,child,Child")]
    public async Task<ActionResult<IEnumerable<CompletedReadingLogDto>>> GetCompletedStoriesByChild(Guid childProfileId)
    {
        try
        {
            var result = await _readingLogService.GetCompletedStoriesByChildAsync(childProfileId);
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
