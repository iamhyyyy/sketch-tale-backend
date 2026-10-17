using Microsoft.AspNetCore.Http;
using sketch_tale.Application.DTOs;

namespace sketch_tale.Application.Interfaces.Services;

public interface IStoryQuizTemplateService
{
    Task<IEnumerable<StoryQuizTemplateDto>> GetQuizzesByStoryAsync(Guid storyId);
    Task<IEnumerable<StoryQuizTemplateDto>> GetAllQuizzesByStoryAsync(Guid storyId);
    Task<StoryQuizTemplateDto?> GetQuizByIdAsync(Guid quizId);
    Task<StoryQuizTemplateDto> CreateQuizAsync(Guid storyId, CreateStoryQuizTemplateDto dto, IFormFile? file = null, IFormFile? audioFile = null);
    Task<bool> UpdateQuizAsync(Guid quizId, UpdateStoryQuizTemplateDto dto, IFormFile? file = null, IFormFile? audioFile = null);
    Task<bool> DeleteQuizAsync(Guid quizId);
}
