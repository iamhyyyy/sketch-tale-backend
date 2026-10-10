using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Services;

public class StoryTemplateService : IStoryTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;
    public StoryTemplateService(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<IEnumerable<StoryTemplateDto>> GetAllAsync()
    {
        var items = await _unitOfWork.StoryTemplateRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<StoryTemplateDto>>(items);
    }

    public async Task<StoryTemplateDto?> GetByIdAsync(Guid id)
    {
        var item = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(id);
        return _mapper.Map<StoryTemplateDto>(item);
    }

    public async Task<StoryTemplateDto> CreateAsync(CreateStoryTemplateDto dto, IFormFile? file)
    {
        var entity = _mapper.Map<StoryTemplate>(dto);
        entity.Id = Guid.NewGuid();
        entity.Status = Domain.Enums.CommonStatus.Pending;
        if (file != null)
            entity.CoverImageUrl = await _cloudinaryService.UploadImageAsync(file, "SketchTale/cover-image");
        await _unitOfWork.StoryTemplateRepository.AddAsync(entity);
        await _unitOfWork.CompleteAsync();

        // 5. Map Entity vừa tạo → DTO trả về cho Controller
        return _mapper.Map<StoryTemplateDto>(entity);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateStoryTemplateDto dto, IFormFile? file)
    {
        var item = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(id);
        if (item == null) return false;
        _mapper.Map(dto, item);
        if (file != null)
            item.CoverImageUrl = await _cloudinaryService.UploadImageAsync(file, "SketchTale/cover-image");
        

        _unitOfWork.StoryTemplateRepository.Update(item);
        await _unitOfWork.CompleteAsync();
        return true;
    }


    public async Task<PublishStoryResultDto> PublishStoryAsync(Guid storyId)
    {
        var story = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(storyId);
        if (story == null)
        {
            throw new KeyNotFoundException($"Story template with id {storyId} was not found.");
        }

        story.Status = Domain.Enums.CommonStatus.Active;
        _unitOfWork.StoryTemplateRepository.Update(story);
        await _unitOfWork.CompleteAsync();

        return new PublishStoryResultDto
        {
            StoryTemplateId = story.Id,
            Title = story.Title,
            Status = story.Status,
            Message = "Story template published successfully."
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var story = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(id);
        if (story == null) return false;

        _unitOfWork.StoryTemplateRepository.Delete(story);
        await _unitOfWork.CompleteAsync();
        return true;
    }


}
