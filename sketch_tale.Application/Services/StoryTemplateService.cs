using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.Services;

public class StoryTemplateService : IStoryTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ICurrentUserService _currentUserService;

    public StoryTemplateService(
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
    //Này lấy tất cả Templete( tất cả status )
    public async Task<IEnumerable<StoryTemplateDto>> GetAllAsync()
    {
        var items = await _unitOfWork.StoryTemplateRepository.GetAllAsync();
        var activeItems = items.Where(s => s.Status != Domain.Enums.CommonStatus.Inactive);
        return _mapper.Map<IEnumerable<StoryTemplateDto>>(activeItems);
    }
    //này lấy tất cả Templete active
    public async Task<IEnumerable<StoryTemplateDto>> GetStories()
    {
        var stories = await _unitOfWork.StoryTemplateRepository.getStories();
        return _mapper.Map<IEnumerable<StoryTemplateDto>>(stories); 
    }
    //getByEduthemeID
    public async Task<IEnumerable<StoryTemplateDto>> GetStoriesByEduThemeId(Guid EduThemeId)
    {
        var stories = await _unitOfWork.StoryTemplateRepository.getByEducationThemeId(EduThemeId);
        return _mapper.Map<IEnumerable<StoryTemplateDto>>(stories);
    }
    public async Task<StoryTemplateDto?> GetByIdAsync(Guid id)
    {
        var item = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(id);
        if (item == null || item.Status == Domain.Enums.CommonStatus.Inactive) return null;
        return _mapper.Map<StoryTemplateDto>(item);
    }

    public async Task<StoryTemplateDto> CreateAsync(CreateStoryTemplateDto dto, IFormFile? file)
    {
        var entity = _mapper.Map<StoryTemplate>(dto);
        entity.Status = Domain.Enums.CommonStatus.Draft;
        if (file != null)
        {
            entity.CoverImageUrl = await _cloudinaryService.UploadImageAsync(file, "SketchTale/cover-image");
            await SaveImageToContentManagerGalleryAsync(entity.CoverImageUrl);
        }
        else if (!string.IsNullOrWhiteSpace(dto.CoverImageUrl))
        {
            entity.CoverImageUrl = dto.CoverImageUrl.Trim();
        }

        await _unitOfWork.StoryTemplateRepository.AddAsync(entity);
        await _unitOfWork.CompleteAsync();
        return _mapper.Map<StoryTemplateDto>(entity);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateStoryTemplateDto dto, IFormFile? file)
    {
        var item = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(id);
        if (item == null || item.Status == Domain.Enums.CommonStatus.Inactive) return false;

        var oldCoverImageUrl = item.CoverImageUrl;
        _mapper.Map(dto, item);

        if (file != null)
        {
            item.CoverImageUrl = await _cloudinaryService.UploadImageAsync(file, "SketchTale/cover-image");
            await SaveImageToContentManagerGalleryAsync(item.CoverImageUrl);
        }
        else if (!string.IsNullOrWhiteSpace(dto.CoverImageUrl))
        {
            item.CoverImageUrl = dto.CoverImageUrl.Trim();
        }
        else
        {
            item.CoverImageUrl = oldCoverImageUrl;
        }

        _unitOfWork.StoryTemplateRepository.Update(item);
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


    public async Task<PublishStoryResultDto> PublishStoryAsync(Guid storyId)
    {
        var story = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(storyId);
        if (story == null || story.Status == Domain.Enums.CommonStatus.Inactive)
        {
            throw new KeyNotFoundException($"Story template with id {storyId} was not found.");
        }

        // 1. Kiểm tra ảnh bìa
        if (string.IsNullOrWhiteSpace(story.CoverImageUrl))
        {
            throw new InvalidOperationException("Câu chuyện cần có ảnh bìa (CoverImageUrl) trước khi xuất bản.");
        }

        // 2. Lấy danh sách trang và kiểm tra số trang
        var allPages = (await _unitOfWork.StoryPageTemplateRepository.GetAllPagesByStoryIdAsync(storyId)).ToList();
        var validPages = allPages.Where(p => p.Status != Domain.Enums.CommonStatus.Inactive).OrderBy(p => p.PageNumber).ToList();

        if (!validPages.Any())
        {
            throw new InvalidOperationException("Câu chuyện phải có ít nhất 1 trang truyện trước khi xuất bản.");
        }

        // 3. Cascade Publish: Cập nhật các StoryPageTemplate và CharacterSlotTemplate từ Draft -> Active
        int publishedPagesCount = 0;
        int publishedSlotsCount = 0;
        foreach (var page in validPages)
        {
            if (page.Status == Domain.Enums.CommonStatus.Draft)
            {
                page.Status = Domain.Enums.CommonStatus.Active;
                page.UpdatedAt = DateTime.UtcNow.AddHours(7);
                _unitOfWork.StoryPageTemplateRepository.Update(page);
                publishedPagesCount++;
            }

            var slots = (await _unitOfWork.CharacterSlotTemplateRepository.GetSlotsByPageIdAsync(page.Id)).ToList();
            foreach (var slot in slots)
            {
                if (slot.Status == Domain.Enums.CommonStatus.Draft)
                {
                    slot.Status = Domain.Enums.CommonStatus.Active;
                    slot.UpdatedAt = DateTime.UtcNow.AddHours(7);
                    _unitOfWork.CharacterSlotTemplateRepository.Update(slot);
                    publishedSlotsCount++;
                }
            }
        }

        // 4. Cascade Publish: Cập nhật các StoryRoleTemplate từ Draft -> Active
        int publishedRolesCount = 0;
        var allRoles = (await _unitOfWork.StoryRoleTemplateRepository.GetAllRolesByStoryIdAsync(storyId)).ToList();
        foreach (var role in allRoles.Where(r => r.Status == Domain.Enums.CommonStatus.Draft))
        {
            role.Status = Domain.Enums.CommonStatus.Active;
            role.UpdatedAt = DateTime.UtcNow.AddHours(7);
            _unitOfWork.StoryRoleTemplateRepository.Update(role);
            publishedRolesCount++;
        }

        // 5. Cascade Publish: Cập nhật các StoryQuizTemplate từ Draft -> Active
        int publishedQuizzesCount = 0;
        var allQuizzes = (await _unitOfWork.StoryQuizTemplateRepository.GetAllQuizzesByStoryIdAsync(storyId)).ToList();
        foreach (var quiz in allQuizzes.Where(q => q.Status == Domain.Enums.CommonStatus.Draft))
        {
            quiz.Status = Domain.Enums.CommonStatus.Active;
            quiz.UpdatedAt = DateTime.UtcNow.AddHours(7);
            _unitOfWork.StoryQuizTemplateRepository.Update(quiz);
            publishedQuizzesCount++;
        }

        // 6. Cập nhật StoryTemplate: Status = Active, đồng bộ TotalPageNumbers
        story.Status = Domain.Enums.CommonStatus.Active;
        story.TotalPageNumbers = validPages.Count;
        story.UpdatedAt = DateTime.UtcNow.AddHours(7);
        _unitOfWork.StoryTemplateRepository.Update(story);

        await _unitOfWork.CompleteAsync();

        return new PublishStoryResultDto
        {
            StoryTemplateId = story.Id,
            Title = story.Title,
            Status = story.Status,
            TotalPages = validPages.Count,
            PublishedPagesCount = publishedPagesCount,
            PublishedRolesCount = publishedRolesCount,
            PublishedSlotsCount = publishedSlotsCount,
            PublishedQuizzesCount = publishedQuizzesCount,
            Message = "Story template and all its child components published successfully."
        };
    }

    public async Task<PublishStoryResultDto> UnpublishStoryAsync(Guid storyId)
    {
        var story = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(storyId);
        if (story == null || story.Status == Domain.Enums.CommonStatus.Inactive)
        {
            throw new KeyNotFoundException($"Story template with id {storyId} was not found.");
        }

        var allPages = (await _unitOfWork.StoryPageTemplateRepository.GetAllPagesByStoryIdAsync(storyId)).ToList();
        var validPages = allPages.Where(p => p.Status != Domain.Enums.CommonStatus.Inactive).ToList();

        foreach (var page in validPages)
        {
            if (page.Status == Domain.Enums.CommonStatus.Active)
            {
                page.Status = Domain.Enums.CommonStatus.Draft;
                page.UpdatedAt = DateTime.UtcNow.AddHours(7);
                _unitOfWork.StoryPageTemplateRepository.Update(page);
            }

            var slots = (await _unitOfWork.CharacterSlotTemplateRepository.GetSlotsByPageIdAsync(page.Id)).ToList();
            foreach (var slot in slots)
            {
                if (slot.Status == Domain.Enums.CommonStatus.Active)
                {
                    slot.Status = Domain.Enums.CommonStatus.Draft;
                    slot.UpdatedAt = DateTime.UtcNow.AddHours(7);
                    _unitOfWork.CharacterSlotTemplateRepository.Update(slot);
                }
            }
        }

        var allRoles = (await _unitOfWork.StoryRoleTemplateRepository.GetAllRolesByStoryIdAsync(storyId)).ToList();
        foreach (var role in allRoles.Where(r => r.Status == Domain.Enums.CommonStatus.Active))
        {
            role.Status = Domain.Enums.CommonStatus.Draft;
            role.UpdatedAt = DateTime.UtcNow.AddHours(7);
            _unitOfWork.StoryRoleTemplateRepository.Update(role);
        }

        var allQuizzes = (await _unitOfWork.StoryQuizTemplateRepository.GetAllQuizzesByStoryIdAsync(storyId)).ToList();
        foreach (var quiz in allQuizzes.Where(q => q.Status == Domain.Enums.CommonStatus.Active))
        {
            quiz.Status = Domain.Enums.CommonStatus.Draft;
            quiz.UpdatedAt = DateTime.UtcNow.AddHours(7);
            _unitOfWork.StoryQuizTemplateRepository.Update(quiz);
        }

        story.Status = Domain.Enums.CommonStatus.Draft;
        story.UpdatedAt = DateTime.UtcNow.AddHours(7);
        _unitOfWork.StoryTemplateRepository.Update(story);

        await _unitOfWork.CompleteAsync();

        return new PublishStoryResultDto
        {
            StoryTemplateId = story.Id,
            Title = story.Title,
            Status = story.Status,
            TotalPages = validPages.Count,
            Message = "Story template unpublished successfully (returned to Draft)."
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var story = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(id);
        if (story == null || story.Status == Domain.Enums.CommonStatus.Inactive) return false;

        story.Status = Domain.Enums.CommonStatus.Inactive;
        story.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.StoryTemplateRepository.Update(story);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    
}
