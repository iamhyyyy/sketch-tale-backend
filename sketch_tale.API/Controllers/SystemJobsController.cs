using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Services;

namespace sketch_tale.WebApi.Controllers;

[ApiController]
[Route("api/system-jobs")]
[AllowAnonymous] // Bỏ qua JWT của user thường để dùng Secret Header riêng
public class SystemJobsController : ControllerBase
{
    private readonly IChildProfileService _childProfileService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SystemJobsController> _logger;

    public SystemJobsController(
        IChildProfileService childProfileService,
        IConfiguration configuration,
        ILogger<SystemJobsController> logger)
    {
        _childProfileService = childProfileService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("reset-daily-limits")]
    public async Task<IActionResult> ResetDailyLimits(
        [FromHeader(Name = "X-Cron-Secret")] string? cronSecret,
        CancellationToken cancellationToken)
    {
        var expectedSecret = _configuration["CronSettings:SecretKey"];

        if (string.IsNullOrEmpty(expectedSecret) || cronSecret != expectedSecret)
        {
            _logger.LogWarning("Unauthorized attempt to trigger daily limit reset.");
            return Unauthorized(new { message = "Invalid cron secret." });
        }

        var updatedCount = await _childProfileService.ResetAllDailyLimitsAsync();
        _logger.LogInformation("Daily limits reset completed for {Count} child profiles.", updatedCount);

        return Ok(new
        {
            message = "Reset daily limits successfully.",
            updatedProfiles = updatedCount,
            executedAtUtc = DateTime.UtcNow
        });
    }
}
