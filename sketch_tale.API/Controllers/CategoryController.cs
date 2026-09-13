using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Services;

namespace sketch_tale.WebApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IEmailService _emailService;

        public CategoryController(ICategoryService categoryService, IEmailService emailService)
        {
            _categoryService = categoryService;
            _emailService = emailService;
        }

        [HttpGet("categories")]
        public async Task<ActionResult<List<CategoryDto>>> GetAll()
        {
            var pets = await _categoryService.GetAllAsync();
            return Ok(pets);
        }

        [HttpGet("category/{id}")]
        public async Task<ActionResult<CategoryDto>> GetById(Guid id)
        {
            var vehicle = await _categoryService.GetByIdAsync(id);
            if (vehicle == null) return NotFound();
            return Ok(vehicle);
        }

        [HttpPost("category")]
        public async Task<ActionResult<CreateCategoryDto>> Create(CreateCategoryDto dto)
        {
            var vehicle = await _categoryService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
        }

        [HttpPatch("category/{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateCategoryDto dto)
        {
            var pet = await _categoryService.Update(id, dto);
            if (!pet) return NotFound();
            return NoContent();
        }

        [HttpGet("test")]
        public async Task<IActionResult> SendTest()
        {
            await _emailService.SendEmailAsync(
                "huyndse184016@fpt.edu.vn",
                "Test Mail",
                "Hello from sketch-tale"
            );

            return Ok("Email sent");
        }
    }
}