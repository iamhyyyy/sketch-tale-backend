using sketch_tale.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Repositories
{
    public interface IUserStoryCharacterMappingRepository : IGenericRepository<UserStoryCharacterMapping>
    {
        Task<List<UserStoryCharacterMapping>> GetByCharacterIdAsync(Guid charId);
        Task<List<UserStoryCharacterMapping>> GetByGenerateStoryIdAsync(Guid genStoryId);
    }
}
