using sketch_tale.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Repositories
{
    public interface ICharacterRepository : IGenericRepository<Character>
    {
        Task<List<Character>> GetPendingCharactersByParentIdAsync(Guid parentProfileId);
        Task<Character?> GetCharacterWithDetailsAsync(Guid characterId);
        Task<List<Character>> GetCharactersByChildIdAsync(Guid childProfileId);
    }
}
