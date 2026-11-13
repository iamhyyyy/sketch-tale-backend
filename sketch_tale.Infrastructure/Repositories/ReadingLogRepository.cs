using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public async Task<IEnumerable<ReadingLog>> GetCompletedByChildProfileIdAsync(Guid childProfileId)
    {
        return await _dbSet
            .Where(r => r.ChildProfileId == childProfileId && r.IsCompleted)
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .ToListAsync();
    }

    public async Task<GeneratedStory?> GetGeneratedStoryWithTemplateAsync(Guid generatedStoryId)
    {
        return await _context.GeneratedStories
            .Include(g => g.StoryTemplate)
            .FirstOrDefaultAsync(g => g.Id == generatedStoryId);
    }

    public async Task<ChildDailyUsageLog?> GetDailyUsageLogAsync(Guid childProfileId, DateOnly logDate)
    {
        return await _context.ChildDailyUsageLogs
            .FirstOrDefaultAsync(d => d.ChildProfileId == childProfileId && d.LogDate == logDate);
    }

    public async Task AddDailyUsageLogAsync(ChildDailyUsageLog log)
    {
        await _context.ChildDailyUsageLogs.AddAsync(log);
    }

    public void UpdateDailyUsageLog(ChildDailyUsageLog log)
    {
        _context.ChildDailyUsageLogs.Update(log);
    }
}
