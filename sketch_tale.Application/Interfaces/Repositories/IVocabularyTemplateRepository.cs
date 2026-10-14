using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Interfaces.Repositories;

public interface IVocabularyTemplateRepository : IGenericRepository<VocabularyTemplate>
{
    Task<IEnumerable<VocabularyTemplate>> GetVocabsByPageIdAsync(Guid pageId);
    Task<VocabularyTemplate?> GetByPageAndWordAsync(Guid pageId, string word);
}