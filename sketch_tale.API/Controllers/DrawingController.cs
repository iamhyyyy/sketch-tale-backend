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
            try
            {
                var result = await _workflowService.AnalyzeAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("generate-character")]
        public async Task<IActionResult> GenerateCharacter([FromBody] GenerateCharacterRequest request)
        {
            try
            {
                var result = await _workflowService.GenerateAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
