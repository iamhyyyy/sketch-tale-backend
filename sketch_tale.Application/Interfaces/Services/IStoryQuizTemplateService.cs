using sketch_tale.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Services;

public interface IStoryQuizTemplateService
{
    Task<IEnumerable<StoryQuizTemplateDto>> GetQuizzesByStoryAsync(Guid storyId);
    Task<StoryQuizTemplateDto> CreateQuizAsync(Guid storyId, CreateStoryQuizTemplateDto dto);
    Task<bool> UpdateQuizAsync(Guid quizId, CreateStoryQuizTemplateDto dto);
    Task<bool> DeleteQuizAsync(Guid quizId);
}
