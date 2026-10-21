using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;

namespace sketch_tale.WebApi.Controllers
{
    [ApiController]
    [Route("api/user-story-character-mappings")]
    public class UserStoryCharacterMappingController : ControllerBase
    {
        private readonly IUserStoryCharacterMappingService _userStoryCharacterMappingService;

        public UserStoryCharacterMappingController(IUserStoryCharacterMappingService UserStoryCharacterMappingService)
        {
            _userStoryCharacterMappingService = UserStoryCharacterMappingService;
        } 

        [HttpGet]
        public async Task<ActionResult<List<UserStoryCharacterMappingDto>>> GetAll()
        {
            var items = await _userStoryCharacterMappingService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserStoryCharacterMappingDto>> GetById(Guid id)
        {
            var item = await _userStoryCharacterMappingService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("character/{charId}")]
        public async Task<ActionResult<List<UserStoryCharacterMappingDto>>> GetByCharacterIdAsync(Guid charId)
        {
            var items = await _userStoryCharacterMappingService.GetByCharacterIdAsync(charId);
            return Ok(items);
        }

        [HttpGet("generate-story/{generateStoryId}")]
        public async Task<ActionResult<List<UserStoryCharacterMappingDto>>> GetByGenerateStoryIdAsync(Guid generateStoryId)
        {
            var items = await _userStoryCharacterMappingService.GetByGenerateStoryIdAsync(generateStoryId);
            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult<CreateUserStoryCharacterMappingDto>> Create(CreateUserStoryCharacterMappingDto dto)
        {
            var item = await _userStoryCharacterMappingService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateUserStoryCharacterMappingDto dto)
        {
            var item = await _userStoryCharacterMappingService.Update(id, dto);
            if (!item) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var item = await _userStoryCharacterMappingService.Delete(id);
            if (!item) return NotFound();
            return NoContent();
        }
    }
}