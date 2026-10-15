using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using sketch_tale.Infrastructure.Data;

namespace sketch_tale.Infrastructure.Repositories;

public class StoryQuizTemplateRepository : GenericRepository<StoryQuizTemplate>, IStoryQuizTemplateRepository
{
    public StoryQuizTemplateRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<StoryQuizTemplate>> GetQuizzesByStoryIdAsync(Guid storyId)
    {
        return await _dbSet
            .Where(q => q.StoryTemplateId == storyId && q.Status != CommonStatus.Inactive)
            .ToListAsync();
    }
}
