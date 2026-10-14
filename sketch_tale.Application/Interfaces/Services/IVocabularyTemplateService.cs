using sketch_tale.Application.DTOs;

namespace sketch_tale.Application.Interfaces.Services;

public interface IVocabularyTemplateService
{
    Task<IEnumerable<VocabularyTemplateDto>> GetVocabsByPageAsync(Guid pageId);
    Task<VocabularyTemplateDto?> GetVocabByIdAsync(Guid vocabId);
    Task<VocabularyTemplateDto> CreateVocabAsync(Guid pageId, CreateVocabularyTemplateDto dto);
    Task<bool> UpdateVocabAsync(Guid vocabId, UpdateVocabularyTemplateDto dto);
    Task<bool> DeleteVocabAsync(Guid vocabId);
}
