using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using sketch_tale.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sketch_tale.Infrastructure.Repositories
{
    public class GeneratedStoryRepository : GenericRepository<GeneratedStory>, IGeneratedStoryRepository
    {
        public GeneratedStoryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<GeneratedStory?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.GeneratedStories
                .Include(g => g.StoryTemplate)
                .Include(g => g.UserStoryCharacterMappings)
                    .ThenInclude(m => m.StoryRoleTemplate)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<List<GeneratedStory>> GetByChildProfileIdAsync(Guid childProfileId)
        {
            return await _context.GeneratedStories
                .Include(g => g.StoryTemplate)
                .Include(g => g.UserStoryCharacterMappings)
                    .ThenInclude(m => m.StoryRoleTemplate)
                .Where(g => g.ChildProfileId == childProfileId && g.Status != GeneratedStoryStatus.Hidden)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }
    }
}
