using AutoMapper;
using Microsoft.AspNetCore.Http;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Services;

public class StoryRoleTemplateService : IStoryRoleTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;

    public StoryRoleTemplateService(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<IEnumerable<StoryRoleTemplateDto>> GetRolesByStoryAsync(Guid storyId)
    {
        var roles = await _unitOfWork.StoryRoleTemplateRepository.GetRolesByStoryIdAsync(storyId);
        return _mapper.Map<IEnumerable<StoryRoleTemplateDto>>(roles);
    }

    public async Task<StoryRoleTemplateDto> CreateRoleAsync(Guid storyId, CreateStoryRoleTemplateDto dto, IFormFile? file = null)
    {
        var story = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(storyId);
        if (story == null)
        {
            throw new KeyNotFoundException($"Story template with id {storyId} was not found.");
        }

        var entity = _mapper.Map<StoryRoleTemplate>(dto);
        entity.Id = Guid.NewGuid();
        entity.StoryTemplateId = storyId;
        entity.Status = Domain.Enums.CommonStatus.Active;

        if (file != null)
        {
            entity.DefaultAssetUrl = await _cloudinaryService.UploadImageAsync(file, "SketchTale/roles");
        }

        await _unitOfWork.StoryRoleTemplateRepository.AddAsync(entity);
        await _unitOfWork.CompleteAsync();

        return _mapper.Map<StoryRoleTemplateDto>(entity);
    }

    public async Task<bool> UpdateRoleAsync(Guid roleId, UpdateStoryRoleTemplateDto dto, IFormFile? file = null)
    {
        var role = await _unitOfWork.StoryRoleTemplateRepository.GetByIdAsync(roleId);
        if (role == null) return false;

        _mapper.Map(dto, role);

        if (file != null)
        {
            role.DefaultAssetUrl = await _cloudinaryService.UploadImageAsync(file, "SketchTale/roles");
        }

        _unitOfWork.StoryRoleTemplateRepository.Update(role);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<bool> DeleteRoleAsync(Guid roleId)
    {
        var role = await _unitOfWork.StoryRoleTemplateRepository.GetByIdAsync(roleId);
        if (role == null) return false;

        _unitOfWork.StoryRoleTemplateRepository.Delete(role);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
