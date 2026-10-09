using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Interfaces.Repositories
{
    public interface IStoryPageTemplateRepository : IGenericRepository<StoryPageTemplate>
    {
        Task<IEnumerable<StoryPageTemplate>> GetPagesByStoryIdAsync(Guid storyId);
    }
}