using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace sketch_tale.API.Controllers
{
    [ApiController]
    [Route("api/generated-stories")]
    public class GeneratedStoryController : ControllerBase
    {
        private readonly IGeneratedStoryService _generatedStoryService;

        public GeneratedStoryController(IGeneratedStoryService generatedStoryService)
        {
            _generatedStoryService = generatedStoryService;
        }

        [Authorize(Roles = "child,Child,parent,Parent")]
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateStory([FromBody] GenerateStoryRequestDto request)
        {
            try
            {
                var result = await _generatedStoryService.GenerateStoryAsync(request);
                if(result != null) return Ok(result);
                throw new Exception();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
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

        [Authorize(Roles = "child,Child,parent,Parent")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGeneratedStoryById(Guid id)
        {
            try
            {
                var result = await _generatedStoryService.GetGeneratedStoryByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "child,Child,parent,Parent")]
        [HttpGet("child-profiles/{childProfileId}")]
        public async Task<IActionResult> GetGeneratedStoriesByChildProfileId(Guid childProfileId)
        {
            try
            {
                var result = await _generatedStoryService.GetGeneratedStoriesByChildProfileIdAsync(childProfileId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
