using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Infrastructure.Data;

namespace sketch_tale.Infrastructure.Repositories;

public class StoryRoleTemplateRepository : GenericRepository<StoryRoleTemplate>, IStoryRoleTemplateRepository
{
    public StoryRoleTemplateRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<StoryRoleTemplate>> GetRolesByStoryIdAsync(Guid storyId)
    {
        return await _dbSet
            .Where(r => r.StoryTemplateId == storyId)
            .ToListAsync();
    }
}
