using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Services;

namespace sketch_tale.API.Controllers
{
    [ApiController]
    [Route("api/drawings")]
    public class DrawingController : ControllerBase
    {
        private readonly IDrawingCharacterService _workflowService;

        public DrawingController(IDrawingCharacterService workflowService)
        {
            _workflowService = workflowService;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromForm] AnalyzeDrawingRequest request)
        {
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
