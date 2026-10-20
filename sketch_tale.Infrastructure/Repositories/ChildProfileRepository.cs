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

        public async Task<ChildProfile?> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId);
        }
    }
}
