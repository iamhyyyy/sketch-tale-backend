using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using sketch_tale.Infrastructure.Data;

namespace sketch_tale.Infrastructure.Repositories;

public class StoryPageTemplateRepository : GenericRepository<StoryPageTemplate>, IStoryPageTemplateRepository
{
    public StoryPageTemplateRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<StoryPageTemplate>> GetPagesByStoryIdAsync(Guid storyId)
    {
        return await _dbSet
            .Where(p => p.StoryTemplateId == storyId && p.Status != CommonStatus.Inactive)
            .OrderBy(p => p.PageNumber)
            .ToListAsync();
    }
    public async Task<IEnumerable<StoryPageTemplate>> GetAllPagesByStoryIdAsync(Guid storyId)
    {
        return await _dbSet
            .Where(p => p.StoryTemplateId == storyId)
            .OrderBy(p => p.PageNumber)
            .ToListAsync();
    }
}
