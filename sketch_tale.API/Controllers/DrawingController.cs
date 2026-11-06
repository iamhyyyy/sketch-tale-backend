using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Services;

namespace sketch_tale.API.Controllers
{
    [ApiController]
    [Route("api/drawings")]
    public class DrawingController : ControllerBase
    {
        private readonly IDrawingCharacterWorkflowService _workflowService;

        public DrawingController(IDrawingCharacterWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromForm] AnalyzeDrawingRequest request)
        {
            // Có thể thêm check số dư Credit phụ huynh ở đây (>= 180)[cite: 1]
            var result = await _workflowService.AnalyzeAsync(request);
            return Ok(result);
        }

        [HttpPost("generate-character")]
        public async Task<IActionResult> GenerateCharacter([FromBody] GenerateCharacterRequest request)
        {
            var result = await _workflowService.GenerateAsync(request);
            return Ok(result);
        }
    }
}
