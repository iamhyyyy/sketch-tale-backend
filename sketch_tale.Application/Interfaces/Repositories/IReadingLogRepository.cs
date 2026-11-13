using sketch_tale.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Repositories;

public interface IReadingLogRepository : IGenericRepository<ReadingLog>
{
    Task<ReadingLog?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<ReadingLog>> GetCompletedByChildProfileIdAsync(Guid childProfileId);
    Task<GeneratedStory?> GetGeneratedStoryWithTemplateAsync(Guid generatedStoryId);
    Task<ChildDailyUsageLog?> GetDailyUsageLogAsync(Guid childProfileId, DateOnly logDate);
    Task AddDailyUsageLogAsync(ChildDailyUsageLog log);
    void UpdateDailyUsageLog(ChildDailyUsageLog log);
}
