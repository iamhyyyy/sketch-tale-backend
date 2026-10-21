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
    public class ContentReportRepository : GenericRepository<ContentReport>, IContentReportRepository
    {
        public ContentReportRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<ContentReport>> GetByParentIdAsync(Guid parentId)
        {
            return await _context.ContentReports
                .Where(c => c.CreateBy == parentId)
                .ToListAsync();
        }
    }
}
