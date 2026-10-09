using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Interfaces.Repositories;

public interface IStoryRoleTemplateRepository : IGenericRepository<StoryRoleTemplate>
{
    Task<IEnumerable<StoryRoleTemplate>> GetRolesByStoryIdAsync(Guid storyId);
}