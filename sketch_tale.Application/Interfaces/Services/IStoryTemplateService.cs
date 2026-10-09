using Microsoft.AspNetCore.Http;
using sketch_tale.Application.DTOs;

namespace sketch_tale.Application.Interfaces.Services;

public interface IStoryTemplateService
{
    //Story Template CRUD
    Task<IEnumerable<StoryTemplateDto>> GetAllAsync();
    Task<StoryTemplateDto?> GetByIdAsync(Guid id);
    Task<StoryTemplateDto> CreateAsync(CreateStoryTemplateDto dto, IFormFile? file);
    Task<bool> UpdateAsync(Guid id, UpdateStoryTemplateDto dto, IFormFile? file);
    Task<bool> DeleteAsync(Guid id);

    //Publish
    Task<PublishStoryResultDto> PublishStoryAsync(Guid storyId);
}
