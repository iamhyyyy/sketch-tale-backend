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
    public class UserStoryCharacterMappingRepository : GenericRepository<UserStoryCharacterMapping>, IUserStoryCharacterMappingRepository
    {
        public UserStoryCharacterMappingRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<UserStoryCharacterMapping>> GetByCharacterIdAsync(Guid charId)
        {
            return await _context.UserStoryCharacterMappings
                .Where(c => c.CharacterId == charId)
                .ToListAsync();
        }
        public async Task<List<UserStoryCharacterMapping>> GetByGenerateStoryIdAsync(Guid genStoryId)
        {
            return await _context.UserStoryCharacterMappings
                .Where(c => c.GeneratedStoryId == genStoryId)
                .ToListAsync();
        }
    }
}
