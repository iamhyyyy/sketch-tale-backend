using sketch_tale.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace sketch_tale.WebApi.Controllers
{
    /// <summary>
    /// Controller xử lý upload ảnh lên Cloudinary.
    /// Dùng chung cho Pet, Item, User avatar.
    /// </summary>
    [ApiController]
    [Route("api/upload")]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;

        public UploadController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("background-image")]
        [Authorize(Roles = "content manager")]
        public async Task<IActionResult> UploadItemImage(IFormFile file)
        {
            return await UploadImage(file, "SketchTale/background-image");
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            return await UploadImage(file, "SketchTale/avatars");
        }

        private async Task<IActionResult> UploadImage(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "Vui lòng chọn file ảnh." });

            // Chỉ chấp nhận file ảnh
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                return BadRequest(new { success = false, message = "Chỉ chấp nhận file JPG, PNG, WEBP, GIF." });

            // Giới hạn 10MB
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { success = false, message = "File không được vượt quá 10MB." });

            try
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(file, folder);
                return Ok(new { success = true, imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
