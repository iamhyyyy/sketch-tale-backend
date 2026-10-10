using sketch_tale.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Services;

public interface IVocabularyTemplateService
{

    //Vocabulary
    Task<IEnumerable<VocabularyTemplateDto>> GetVocabsByPageAsync(Guid pageId);
    Task<VocabularyTemplateDto> CreateVocabAsync(Guid pageId, CreateVocabularyTemplateDto dto);
    Task<bool> DeleteVocabAsync(Guid vocabId);

}
