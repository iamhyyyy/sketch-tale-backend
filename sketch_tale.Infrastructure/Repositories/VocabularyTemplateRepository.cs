using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Infrastructure.Data;

namespace sketch_tale.Infrastructure.Repositories;

public class VocabularyTemplateRepository : GenericRepository<VocabularyTemplate>, IVocabularyTemplateRepository
{
    public VocabularyTemplateRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<VocabularyTemplate>> GetVocabsByPageIdAsync(Guid pageId)
    {
        return await _dbSet
            .Where(v => v.StoryPageTemplateId == pageId)
            .ToListAsync();
    }

    public async Task<VocabularyTemplate?> GetByPageAndWordAsync(Guid pageId, string word)
    {
        return await _dbSet
            .FirstOrDefaultAsync(v => v.StoryPageTemplateId == pageId && v.Word.ToLower() == word.Trim().ToLower());
    }
}
