using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Interfaces;
using sketch_tale.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Infrastructure.Repositories
{
    public class ChildProfileRepository : GenericRepository<ChildProfile>, IChildProfileRepository
    {
        public ChildProfileRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<ChildProfile>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.AllowedThemes)
                .Include(c => c.StoryTemplateRecommends)
                .ToListAsync();
        }

        public override async Task<ChildProfile?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(c => c.AllowedThemes)
                .Include(c => c.StoryTemplateRecommends)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ChildProfile?> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(c => c.AllowedThemes)
                .Include(c => c.StoryTemplateRecommends)
                .FirstOrDefaultAsync(c => c.Id == userId);
        }
    }
}
