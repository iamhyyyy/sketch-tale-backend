using AutoMapper;
using Microsoft.AspNetCore.Http;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.Services;

public class StoryRoleTemplateService : IStoryRoleTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ICurrentUserService _currentUserService;

    public StoryRoleTemplateService(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        ICloudinaryService cloudinaryService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cloudinaryService = cloudinaryService;
        _currentUserService = currentUserService;
    }
    //l��y active
    public async Task<IEnumerable<StoryRoleTemplateDto>> GetRolesByStoryAsync(Guid storyId)
    {
        var roles = await _unitOfWork.StoryRoleTemplateRepository.GetRolesByStoryIdAsync(storyId);
        return _mapper.Map<IEnumerable<StoryRoleTemplateDto>>(roles);
    }
    //l��y all t��t ca� status
    public async Task<IEnumerable<StoryRoleTemplateDto>> GetAllRolesByStoryAsync(Guid storyId)
    {
        var roles = await _unitOfWork.StoryRoleTemplateRepository.GetAllRolesByStoryIdAsync(storyId);
        return _mapper.Map<IEnumerable<StoryRoleTemplateDto>>(roles);
    }
    //l��y role b��ng id
    public async Task<StoryRoleTemplateDto> GetRoleByIdAsync(Guid RoleId)
    {
        var role = await _unitOfWork.StoryRoleTemplateRepository.GetByIdAsync(RoleId);
        return _mapper.Map<StoryRoleTemplateDto>(role);
    }

    public async Task<StoryRoleTemplateDto> CreateRoleAsync(Guid storyId, CreateStoryRoleTemplateDto dto, IFormFile? file = null)
    {
        var story = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(storyId);
        if (story == null || story.Status == Domain.Enums.CommonStatus.Inactive)
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
            await SaveImageToContentManagerGalleryAsync(entity.DefaultAssetUrl);
        }
        else if (!string.IsNullOrWhiteSpace(dto.DefaultAssetUrl))
        {
            entity.DefaultAssetUrl = dto.DefaultAssetUrl.Trim();
        }

        await _unitOfWork.StoryRoleTemplateRepository.AddAsync(entity);
        await _unitOfWork.CompleteAsync();

        return _mapper.Map<StoryRoleTemplateDto>(entity);
    }

    public async Task<bool> UpdateRoleAsync(Guid roleId, UpdateStoryRoleTemplateDto dto, IFormFile? file = null)
    {
        var role = await _unitOfWork.StoryRoleTemplateRepository.GetByIdAsync(roleId);
        if (role == null || role.Status == Domain.Enums.CommonStatus.Inactive) return false;

        var oldAssetUrl = role.DefaultAssetUrl;

        _mapper.Map(dto, role);

        if (file != null)
        {
            role.DefaultAssetUrl = await _cloudinaryService.UploadImageAsync(file, "SketchTale/roles");
            await SaveImageToContentManagerGalleryAsync(role.DefaultAssetUrl);
        }
        else if (!string.IsNullOrWhiteSpace(dto.DefaultAssetUrl))
        {
            role.DefaultAssetUrl = dto.DefaultAssetUrl.Trim();
        }
        else
        {
            role.DefaultAssetUrl = oldAssetUrl;
        }

        role.UpdatedAt = DateTime.UtcNow.AddHours(7);

        _unitOfWork.StoryRoleTemplateRepository.Update(role);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    private async Task SaveImageToContentManagerGalleryAsync(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || !Guid.TryParse(_currentUserService.UserId, out var cmId))
            return;

        var cmProfile = await _unitOfWork.Repository<ContentManagerProfile>().GetByIdAsync(cmId);
        if (cmProfile != null)
        {
            cmProfile.UploadImageUrl ??= new List<string>();
            if (!cmProfile.UploadImageUrl.Contains(imageUrl))
            {
                cmProfile.UploadImageUrl.Add(imageUrl);
                cmProfile.UpdatedAt = DateTime.UtcNow.AddHours(7);
                cmProfile.UpdateBy = cmId;
                _unitOfWork.Repository<ContentManagerProfile>().Update(cmProfile);
            }
        }
        else
        {
            cmProfile = new ContentManagerProfile
            {
                Id = cmId,
                UploadImageUrl = new List<string> { imageUrl },
                CreatedAt = DateTime.UtcNow.AddHours(7),
                CreateBy = cmId
            };
            await _unitOfWork.Repository<ContentManagerProfile>().AddAsync(cmProfile);
        }
    }

    public async Task<bool> DeleteRoleAsync(Guid roleId)
    {
        var role = await _unitOfWork.StoryRoleTemplateRepository.GetByIdAsync(roleId);
        if (role == null || role.Status == Domain.Enums.CommonStatus.Inactive) return false;

        role.Status = Domain.Enums.CommonStatus.Inactive;
        role.UpdatedAt = DateTime.UtcNow.AddHours(7);

        _unitOfWork.StoryRoleTemplateRepository.Update(role);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    
}
