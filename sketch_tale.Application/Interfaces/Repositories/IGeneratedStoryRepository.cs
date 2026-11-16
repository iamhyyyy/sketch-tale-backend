using sketch_tale.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Repositories
{
    public interface IGeneratedStoryRepository : IGenericRepository<GeneratedStory>
    {
        Task<GeneratedStory?> GetByIdWithDetailsAsync(Guid id);
        Task<List<GeneratedStory>> GetByChildProfileIdAsync(Guid childProfileId);
    }
}
