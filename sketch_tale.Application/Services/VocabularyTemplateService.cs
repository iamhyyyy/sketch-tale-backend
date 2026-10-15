using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Services;

public class VocabularyTemplateService : IVocabularyTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VocabularyTemplateService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<VocabularyTemplateDto>> GetVocabsByPageAsync(Guid pageId)
    {
        var page = await _unitOfWork.StoryPageTemplateRepository.GetByIdAsync(pageId);
        if (page == null || page.Status == sketch_tale.Domain.Enums.CommonStatus.Inactive)
        {
            throw new KeyNotFoundException($"Trang truyện với Id {pageId} không tồn tại.");
        }

        var vocabs = await _unitOfWork.VocabularyTemplateRepository.GetVocabsByPageIdAsync(pageId);
        return _mapper.Map<IEnumerable<VocabularyTemplateDto>>(vocabs);
    }

    public async Task<VocabularyTemplateDto?> GetVocabByIdAsync(Guid vocabId)
    {
        var vocab = await _unitOfWork.VocabularyTemplateRepository.GetByIdAsync(vocabId);
        if (vocab == null) return null;

        return _mapper.Map<VocabularyTemplateDto>(vocab);
    }

    public async Task<VocabularyTemplateDto> CreateVocabAsync(Guid pageId, CreateVocabularyTemplateDto dto)
    {
        var page = await _unitOfWork.StoryPageTemplateRepository.GetByIdAsync(pageId);
        if (page == null || page.Status == sketch_tale.Domain.Enums.CommonStatus.Inactive)
        {
            throw new KeyNotFoundException($"Trang truyện với Id {pageId} không tồn tại.");
        }

        if (string.IsNullOrWhiteSpace(dto.Word))
        {
            throw new ArgumentException("Từ vựng không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(dto.Meaning))
        {
            throw new ArgumentException("Nghĩa của từ vựng không được để trống.");
        }

        var trimmedWord = dto.Word.Trim();
        var existing = await _unitOfWork.VocabularyTemplateRepository.GetByPageAndWordAsync(pageId, trimmedWord);
        if (existing != null)
        {
            throw new InvalidOperationException($"Từ vựng '{trimmedWord}' đã tồn tại trên trang này.");
        }

        var entity = _mapper.Map<VocabularyTemplate>(dto);
        entity.Id = Guid.NewGuid();
        entity.StoryPageTemplateId = pageId;
        entity.Word = trimmedWord;
        entity.Meaning = dto.Meaning.Trim();
        entity.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.VocabularyTemplateRepository.AddAsync(entity);
        await _unitOfWork.CompleteAsync();

        return _mapper.Map<VocabularyTemplateDto>(entity);
    }

    public async Task<bool> UpdateVocabAsync(Guid vocabId, UpdateVocabularyTemplateDto dto)
    {
        var vocab = await _unitOfWork.VocabularyTemplateRepository.GetByIdAsync(vocabId);
        if (vocab == null) return false;

        if (!string.IsNullOrWhiteSpace(dto.Word))
        {
            var trimmedWord = dto.Word.Trim();
            if (!string.Equals(vocab.Word, trimmedWord, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _unitOfWork.VocabularyTemplateRepository.GetByPageAndWordAsync(vocab.StoryPageTemplateId, trimmedWord);
                if (existing != null && existing.Id != vocabId)
                {
                    throw new InvalidOperationException($"Từ vựng '{trimmedWord}' đã tồn tại trên trang này.");
                }
                vocab.Word = trimmedWord;
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.Meaning))
        {
            vocab.Meaning = dto.Meaning.Trim();
        }

        if (dto.AudioUrl != null)
        {
            vocab.AudioUrl = dto.AudioUrl;
        }

        vocab.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.VocabularyTemplateRepository.Update(vocab);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<bool> DeleteVocabAsync(Guid vocabId)
    {
        var vocab = await _unitOfWork.VocabularyTemplateRepository.GetByIdAsync(vocabId);
        if (vocab == null) return false;

        _unitOfWork.VocabularyTemplateRepository.Delete(vocab);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
