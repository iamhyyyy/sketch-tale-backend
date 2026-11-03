using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sketch_tale.Infrastructure.Repositories;

public class ChildQuizAnswerRepository : GenericRepository<ChildQuizAnswer>, IChildQuizAnswerRepository
{
    public ChildQuizAnswerRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ChildQuizAnswer>> GetAnswersByReadingLogIdAsync(Guid readingLogId)
    {
        return await _dbSet
            .Include(a => a.StoryQuizTemplate)
            .Where(a => a.ReadingLogId == readingLogId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<ChildQuizAnswer?> GetAnswerAsync(Guid readingLogId, Guid storyQuizTemplateId)
    {
        return await _dbSet
            .Include(a => a.StoryQuizTemplate)
            .FirstOrDefaultAsync(a => a.ReadingLogId == readingLogId && a.StoryQuizTemplateId == storyQuizTemplateId);
    }

    public async Task<IEnumerable<ChildQuizAnswer>> GetAnswersByChildProfileIdAsync(Guid childProfileId)
    {
        return await _dbSet
            .Include(a => a.StoryQuizTemplate)
            .Include(a => a.ReadingLog)
            .Where(a => a.ChildProfileId == childProfileId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}
