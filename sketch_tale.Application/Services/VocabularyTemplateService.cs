using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Services;

public class VocabularyTemplateService : IVocabularyTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;
    public VocabularyTemplateService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public Task<VocabularyTemplateDto> CreateVocabAsync(Guid pageId, CreateVocabularyTemplateDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteVocabAsync(Guid vocabId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<VocabularyTemplateDto>> GetVocabsByPageAsync(Guid pageId)
    {
        throw new NotImplementedException();
    }
}
