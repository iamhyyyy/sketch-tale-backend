using AutoMapper;
using Microsoft.AspNetCore.Http;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.Services;

public class StoryPageTemplateService : IStoryPageTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;

    public StoryPageTemplateService(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cloudinaryService = cloudinaryService;
    }
    //mới thêm GetAllPage( bao gồm tất cả status )
    public async Task<IEnumerable<StoryPageTemplateDto>> GetAllPagesByStoryAsync(Guid storyId)
    {
        var pages = await _unitOfWork.StoryPageTemplateRepository.GetAllPagesByStoryIdAsync(storyId);
        return _mapper.Map<IEnumerable<StoryPageTemplateDto>>(pages);
    }
    //cái này là getpages nhưng mà chỉ lấy cái nào active thôi
    public async Task<IEnumerable<StoryPageTemplateDto>> GetPagesByStoryAsync(Guid storyId)
    {
        var pages = await _unitOfWork.StoryPageTemplateRepository.GetPagesByStoryIdAsync(storyId);
        return _mapper.Map<IEnumerable<StoryPageTemplateDto>>(pages);
    }
    //lấy page theo ID
    public async Task<StoryPageTemplateDto> GetPageByIdAsync(Guid pageId)
    {
        var page = await _unitOfWork.StoryPageTemplateRepository.GetByIdAsync(pageId);
        return _mapper.Map<StoryPageTemplateDto>(page);
    }

    public async Task<StoryPageTemplateDto> CreatePageAsync(Guid storyId, CreateStoryPageTemplateDto dto, IFormFile? imageFile = null, IFormFile? audioFile = null)
    {
        var story = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(storyId);
        if (story == null || story.Status == Domain.Enums.CommonStatus.Inactive)
        {
            throw new KeyNotFoundException($"Story template with id {storyId} was not found.");
        }

        var entity = _mapper.Map<StoryPageTemplate>(dto);
        entity.Id = Guid.NewGuid();
        entity.StoryTemplateId = storyId;

        // Tự động tăng và gán số trang tiếp theo
        story.TotalPageNumbers += 1;
        entity.PageNumber = story.TotalPageNumbers;

        if (imageFile != null)
        {
            entity.BackgroundUrl = await _cloudinaryService.UploadImageAsync(imageFile, "SketchTale/background");
        }

        if (audioFile != null)
        {
            entity.AudioNarrationUrl = await _cloudinaryService.UploadAudioAsync(audioFile, "SketchTale/audios");
        }

        entity.Status = Domain.Enums.CommonStatus.Draft;

        await _unitOfWork.StoryPageTemplateRepository.AddAsync(entity);
        _unitOfWork.StoryTemplateRepository.Update(story);
        await _unitOfWork.CompleteAsync();

        return _mapper.Map<StoryPageTemplateDto>(entity);
    }

    public async Task<bool> UpdatePageAsync(Guid pageId, UpdateStoryPageTemplateDto dto, IFormFile? imageFile = null, IFormFile? audioFile = null)
    {
        var page = await _unitOfWork.StoryPageTemplateRepository.GetByIdAsync(pageId);
        if (page == null || page.Status == Domain.Enums.CommonStatus.Inactive) return false;

        _mapper.Map(dto, page);

        if (imageFile != null)
        {
            page.BackgroundUrl = await _cloudinaryService.UploadImageAsync(imageFile, "SketchTale/background");
        }

        if (audioFile != null)
        {
            page.AudioNarrationUrl = await _cloudinaryService.UploadAudioAsync(audioFile, "SketchTale/audios");
        }

        page.UpdatedAt = DateTime.UtcNow.AddHours(7);

        _unitOfWork.StoryPageTemplateRepository.Update(page);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<bool> DeletePageAsync(Guid pageId)
    {
        var page = await _unitOfWork.StoryPageTemplateRepository.GetByIdAsync(pageId);
        if (page == null || page.Status == Domain.Enums.CommonStatus.Inactive) return false;

        var story = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(page.StoryTemplateId);
        if (story != null && story.TotalPageNumbers > 0)
        {
            story.TotalPageNumbers -= 1;
            _unitOfWork.StoryTemplateRepository.Update(story);
        }

        page.Status = Domain.Enums.CommonStatus.Inactive;
        page.UpdatedAt = DateTime.UtcNow.AddHours(7);

        _unitOfWork.StoryPageTemplateRepository.Update(page);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    
}
