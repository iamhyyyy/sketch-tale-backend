using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Infrastructure.Repositories
{
    public class ParentProfileRepository : GenericRepository<ParentProfile>, IParentProfileRepository
    {
        public ParentProfileRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ParentProfile>> GetAllWithDetailsAsync()
        {
            return await _dbSet
             .Include(c => c.ChildProfiles)
             .ToListAsync();
        }

        public async Task<ParentProfile?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(c => c.ChildProfiles)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
   

