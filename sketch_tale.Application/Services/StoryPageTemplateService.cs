using AutoMapper;
using Microsoft.AspNetCore.Http;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;

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

    public async Task<IEnumerable<StoryPageTemplateDto>> GetPagesByStoryAsync(Guid storyId)
    {
        var pages = await _unitOfWork.StoryPageTemplateRepository.GetPagesByStoryIdAsync(storyId);
        return _mapper.Map<IEnumerable<StoryPageTemplateDto>>(pages);
    }

    public async Task<StoryPageTemplateDto> CreatePageAsync(Guid storyId, CreateStoryPageTemplateDto dto, IFormFile? file)
    {
        var story = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(storyId);
        if (story == null)
        {
            throw new KeyNotFoundException($"Story template with id {storyId} was not found.");
        }

        var entity = _mapper.Map<StoryPageTemplate>(dto);
        entity.Id = Guid.NewGuid();
        entity.StoryTemplateId = storyId;

        // Tự động tăng và gán số trang tiếp theo
        story.TotalPageNumbers += 1;
        entity.PageNumber = story.TotalPageNumbers;

        if (file != null)
        {
            entity.BackgroundUrl = await _cloudinaryService.UploadImageAsync(file, "SketchTale/background");
        }

        entity.Status = Domain.Enums.CommonStatus.Draft;

        await _unitOfWork.StoryPageTemplateRepository.AddAsync(entity);
        _unitOfWork.StoryTemplateRepository.Update(story);
        await _unitOfWork.CompleteAsync();

        return _mapper.Map<StoryPageTemplateDto>(entity);
    }

    public async Task<bool> UpdatePageAsync(Guid pageId, UpdateStoryPageTemplateDto dto, IFormFile? file)
    {
        var page = await _unitOfWork.StoryPageTemplateRepository.GetByIdAsync(pageId);
        if (page == null) return false;

        _mapper.Map(dto, page);

        if (file != null)
        {
            page.BackgroundUrl = await _cloudinaryService.UploadImageAsync(file, "SketchTale/background");
        }

        _unitOfWork.StoryPageTemplateRepository.Update(page);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<bool> DeletePageAsync(Guid pageId)
    {
        var page = await _unitOfWork.StoryPageTemplateRepository.GetByIdAsync(pageId);
        if (page == null) return false;

        var story = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(page.StoryTemplateId);
        if (story != null && story.TotalPageNumbers > 0)
        {
            story.TotalPageNumbers -= 1;
            _unitOfWork.StoryTemplateRepository.Update(story);
        }

        _unitOfWork.StoryPageTemplateRepository.Delete(page);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
