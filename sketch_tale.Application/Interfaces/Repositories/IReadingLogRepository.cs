using sketch_tale.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Repositories;

public interface IReadingLogRepository : IGenericRepository<ReadingLog>
{
    Task<ReadingLog?> GetByIdWithDetailsAsync(Guid id);
}
