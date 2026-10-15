using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using sketch_tale.Infrastructure.Data;

namespace sketch_tale.Infrastructure.Repositories;

public class CharacterSlotTemplateRepository : GenericRepository<CharacterSlotTemplate>, ICharacterSlotTemplateRepository
{
    public CharacterSlotTemplateRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CharacterSlotTemplate>> GetSlotsByPageIdAsync(Guid pageId)
    {
        return await _dbSet
            .Where(s => s.StoryPageId == pageId && s.Status != CommonStatus.Inactive)
            .ToListAsync();
    }
}
