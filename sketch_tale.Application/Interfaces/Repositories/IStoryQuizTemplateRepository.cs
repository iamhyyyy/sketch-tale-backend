using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Interfaces.Repositories;

public interface IStoryQuizTemplateRepository : IGenericRepository<StoryQuizTemplate>
{
    Task<IEnumerable<StoryQuizTemplate>> GetQuizzesByStoryIdAsync(Guid storyId);
}