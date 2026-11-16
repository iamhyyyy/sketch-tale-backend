using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace sketch_tale.WebApi.Controllers
{
    [ApiController]
    [Route("api/upload")]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UploadController(
            ICloudinaryService cloudinaryService,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _cloudinaryService = cloudinaryService;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        [HttpPost("background-image")]
        [Authorize(Roles = "content manager, ContentManager")]
        public async Task<IActionResult> UploadItemImage(IFormFile file)
        {
            return await UploadImage(file, "SketchTale/background-image", saveToContentManager: true);
        }

        [HttpPost("content-manager-image")]
        [Authorize(Roles = "content manager, ContentManager")]
        public async Task<IActionResult> UploadContentManagerImage(IFormFile file, [FromQuery] string folder = "SketchTale/content-manager")
        {
            return await UploadImage(file, folder, saveToContentManager: true);
        }

        /// <summary>
        /// Lấy danh sách các hình ảnh đã từng upload của Content Manager để tái sử dụng
        /// </summary>
        [HttpGet("content-manager-images")]
        [Authorize(Roles = "content manager, ContentManager")]
        public async Task<IActionResult> GetContentManagerUploadedImages()
        {
            if (!Guid.TryParse(_currentUserService.UserId, out var cmId))
                return Unauthorized(new { message = "Không xác định được danh tính người dùng." });

            var cmProfile = await _unitOfWork.Repository<ContentManagerProfile>().GetByIdAsync(cmId);
            if (cmProfile == null)
            {
                return Ok(new List<string>());
            }

            return Ok(cmProfile.UploadImageUrl ?? new List<string>());
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            return await UploadImage(file, "SketchTale/avatars", saveToContentManager: false);
        }

        private async Task<IActionResult> UploadImage(IFormFile file, string folder, bool saveToContentManager = false)
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

                // Nếu là Content Manager tải lên, lưu lại vào danh sách UploadImageUrl của ContentManagerProfile
                if (saveToContentManager && Guid.TryParse(_currentUserService.UserId, out var cmId))
                {
                    var cmProfile = await _unitOfWork.Repository<ContentManagerProfile>().GetByIdAsync(cmId);
                    if (cmProfile != null)
                    {
                        cmProfile.UploadImageUrl ??= new List<string>();
                        if (!cmProfile.UploadImageUrl.Contains(imageUrl))
                        {
                            cmProfile.UploadImageUrl.Add(imageUrl);
                            cmProfile.UpdatedAt = DateTime.UtcNow.AddHours(7);
                            cmProfile.UpdateBy = cmId;
                            _unitOfWork.Repository<ContentManagerProfile>().Update(cmProfile);
                            await _unitOfWork.CompleteAsync();
                        }
                    }
                    else
                    {
                        cmProfile = new ContentManagerProfile
                        {
                            Id = cmId,
                            UploadImageUrl = new List<string> { imageUrl },
                            CreatedAt = DateTime.UtcNow.AddHours(7),
                            CreateBy = cmId
                        };
                        await _unitOfWork.Repository<ContentManagerProfile>().AddAsync(cmProfile);
                        await _unitOfWork.CompleteAsync();
                    }
                }

                return Ok(new { success = true, imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
