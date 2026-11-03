using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace sketch_tale.Infrastructure.Repositories;

public class ReadingLogRepository : GenericRepository<ReadingLog>, IReadingLogRepository
{
    public ReadingLogRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<ReadingLog?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(r => r.ChildQuizAnswers)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}
