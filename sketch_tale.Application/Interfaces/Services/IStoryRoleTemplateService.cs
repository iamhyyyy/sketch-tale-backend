using Microsoft.AspNetCore.Http;
using sketch_tale.Application.DTOs;

namespace sketch_tale.Application.Interfaces.Services;

public interface IStoryRoleTemplateService
{
    //Story Role
    Task<IEnumerable<StoryRoleTemplateDto>> GetRolesByStoryAsync(Guid storyId);
    Task<IEnumerable<StoryRoleTemplateDto>> GetAllRolesByStoryAsync(Guid storyId);
    Task<StoryRoleTemplateDto> GetRoleByIdAsync(Guid roleId);
    Task<StoryRoleTemplateDto> CreateRoleAsync(Guid storyId, CreateStoryRoleTemplateDto dto, IFormFile? file = null);
    Task<bool> UpdateRoleAsync(Guid roleId, UpdateStoryRoleTemplateDto dto, IFormFile? file = null);
    Task<bool> DeleteRoleAsync(Guid roleId);
}
