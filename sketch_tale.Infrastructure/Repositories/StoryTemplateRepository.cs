using Microsoft.EntityFrameworkCore;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using sketch_tale.Domain.Interfaces;
using sketch_tale.Infrastructure.Data;

namespace sketch_tale.Infrastructure.Repositories;

public class StoryTemplateRepository : GenericRepository<StoryTemplate>, IStoryTemplateRepository
{
    public StoryTemplateRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<StoryTemplate>> getByEducationThemeId(Guid EduThemeId)
    {
        return await _dbSet
            .Where(p => p.Status != CommonStatus.Inactive && p.EducationThemeId == EduThemeId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<StoryTemplate>> getStories()
    {
        return await _dbSet
            .Where(p => p.Status != CommonStatus.Inactive)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();
    }
}
