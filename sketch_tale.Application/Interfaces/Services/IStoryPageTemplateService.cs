using Microsoft.AspNetCore.Http;
using sketch_tale.Application.DTOs;

namespace sketch_tale.Application.Interfaces.Services;

public interface IStoryPageTemplateService
{
    //Story Page
    Task<IEnumerable<StoryPageTemplateDto>> GetPagesByStoryAsync(Guid storyId);
    Task<StoryPageTemplateDto> CreatePageAsync(Guid storyId, CreateStoryPageTemplateDto dto, IFormFile? imageFile = null, IFormFile? audioFile = null);
    Task<bool> UpdatePageAsync(Guid pageId, UpdateStoryPageTemplateDto dto, IFormFile? imageFile = null, IFormFile? audioFile = null);
    Task<bool> DeletePageAsync(Guid pageId);
}
