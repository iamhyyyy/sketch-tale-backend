using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using sketch_tale.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sketch_tale.Infrastructure.Repositories
{
    public class CharacterRepository : GenericRepository<Character>, ICharacterRepository
    {
        public CharacterRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Character>> GetPendingCharactersByParentIdAsync(Guid parentProfileId)
        {
            var childIds = await _context.ChildProfiles
                .Where(cp => cp.ParentProfileId == parentProfileId)
                .Select(cp => cp.Id)
                .ToListAsync();

            if (!childIds.Any())
                return new List<Character>();

            return await _context.Characters
                .Include(c => c.ChildProfile)
                .Include(c => c.AICharacterGenerate)
                    .ThenInclude(ai => ai.Drawing)
                .Where(c => childIds.Contains(c.ChildProfileId) && c.Status == CharacterStatus.Pending)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Character?> GetCharacterWithDetailsAsync(Guid characterId)
        {
            return await _context.Characters
                .Include(c => c.ChildProfile)
                .Include(c => c.AICharacterGenerate)
                    .ThenInclude(ai => ai.Drawing)
                .FirstOrDefaultAsync(c => c.Id == characterId);
        }
    }
}
