using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sketch_tale.Application.Services;

public class ReadingLogService : IReadingLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public ReadingLogService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ReadingLogDto> StartReadingSessionAsync(StartReadingLogDto dto)
    {
        if (dto.GeneratedStoryId == Guid.Empty)
        {
            throw new ArgumentException("GeneratedStoryId không hợp lệ.");
        }

        var generatedStory = await _unitOfWork.ReadingLogRepository.GetGeneratedStoryWithTemplateAsync(dto.GeneratedStoryId);
        if (generatedStory == null)
        {
            throw new KeyNotFoundException($"Cuốn truyện với Id {dto.GeneratedStoryId} không tồn tại.");
        }

        Guid childProfileId = generatedStory.ChildProfileId;

        if (Guid.TryParse(_currentUserService.UserId, out var loggedInUserId))
        {
            var currentChild = await _unitOfWork.ChildProfileRepository.GetByUserIdAsync(loggedInUserId);
            if (currentChild != null)
            {
                if (generatedStory.ChildProfileId != currentChild.Id)
                {
                    throw new UnauthorizedAccessException("Bé chỉ có thể bắt đầu phiên đọc cuốn truyện của chính mình!");
                }
                childProfileId = currentChild.Id;
            }
        }

        if (generatedStory.LastPageRead <= 0)
        {
            generatedStory.LastPageRead = 1;
            generatedStory.UpdatedAt = DateTime.UtcNow.AddHours(7);
            _unitOfWork.Repository<GeneratedStory>().Update(generatedStory);
        }

        var readingLog = new ReadingLog
        {
            Id = Guid.NewGuid(),
            ChildProfileId = childProfileId,
            GeneratedStoryId = generatedStory.Id,
            IsCompleted = false,
            ReadDurationSeconds = 0,
            CreatedAt = DateTime.UtcNow.AddHours(7)
        };

        await _unitOfWork.ReadingLogRepository.AddAsync(readingLog);
        await _unitOfWork.CompleteAsync();

        return MapToReadingLogDto(readingLog, generatedStory);
    }

    public async Task<ReadingLogDto> UpdateReadingProgressAsync(Guid readingLogId, UpdateReadingProgressDto dto)
    {
        var readingLog = await _unitOfWork.ReadingLogRepository.GetByIdAsync(readingLogId);
        if (readingLog == null)
        {
            throw new KeyNotFoundException($"Phiên đọc truyện với Id {readingLogId} không tồn tại.");
        }

        if (Guid.TryParse(_currentUserService.UserId, out var loggedInUserId))
        {
            var currentChild = await _unitOfWork.ChildProfileRepository.GetByUserIdAsync(loggedInUserId);
            if (currentChild != null && readingLog.ChildProfileId != currentChild.Id)
            {
                throw new UnauthorizedAccessException("Bé chỉ có thể cập nhật tiến độ phiên đọc của chính mình!");
            }
        }

        if (dto.CurrentPage <= 0)
        {
            throw new ArgumentException("Trang hiện tại phải lớn hơn 0.");
        }

        if (dto.ReadDurationSeconds < 0)
        {
            throw new ArgumentException("Thời gian đọc không hợp lệ.");
        }

        var generatedStory = await _unitOfWork.ReadingLogRepository.GetGeneratedStoryWithTemplateAsync(readingLog.GeneratedStoryId);
        if (generatedStory != null)
        {
            if (generatedStory.StoryTemplate != null &&
                generatedStory.StoryTemplate.TotalPageNumbers > 0 &&
                dto.CurrentPage > generatedStory.StoryTemplate.TotalPageNumbers)
            {
                throw new ArgumentException($"Trang hiện tại ({dto.CurrentPage}) vượt quá tổng số trang của truyện ({generatedStory.StoryTemplate.TotalPageNumbers}).");
            }

            generatedStory.LastPageRead = dto.CurrentPage;
            generatedStory.UpdatedAt = DateTime.UtcNow.AddHours(7);
            _unitOfWork.Repository<GeneratedStory>().Update(generatedStory);
        }

        readingLog.ReadDurationSeconds += dto.ReadDurationSeconds;
        readingLog.UpdatedAt = DateTime.UtcNow.AddHours(7);
        _unitOfWork.ReadingLogRepository.Update(readingLog);

        if (dto.ReadDurationSeconds > 0)
        {
            await UpdateDailyUsageAsync(readingLog.ChildProfileId, addedDurationSeconds: dto.ReadDurationSeconds, addedStoryReadCount: 0);
        }

        await _unitOfWork.CompleteAsync();

        return MapToReadingLogDto(readingLog, generatedStory);
    }

    public async Task<ReadingLogDto> CompleteReadingSessionAsync(Guid readingLogId)
    {
        var readingLog = await _unitOfWork.ReadingLogRepository.GetByIdAsync(readingLogId);
        if (readingLog == null)
        {
            throw new KeyNotFoundException($"Phiên đọc truyện với Id {readingLogId} không tồn tại.");
        }

        if (Guid.TryParse(_currentUserService.UserId, out var loggedInUserId))
        {
            var currentChild = await _unitOfWork.ChildProfileRepository.GetByUserIdAsync(loggedInUserId);
            if (currentChild != null && readingLog.ChildProfileId != currentChild.Id)
            {
                throw new UnauthorizedAccessException("Bé chỉ có thể hoàn thành phiên đọc của chính mình!");
            }
        }

        bool wasAlreadyCompleted = readingLog.IsCompleted;

        readingLog.IsCompleted = true;
        readingLog.UpdatedAt = DateTime.UtcNow.AddHours(7);
        _unitOfWork.ReadingLogRepository.Update(readingLog);

        var generatedStory = await _unitOfWork.ReadingLogRepository.GetGeneratedStoryWithTemplateAsync(readingLog.GeneratedStoryId);
        if (generatedStory != null)
        {
            generatedStory.Status = GeneratedStoryStatus.Completed;
            if (generatedStory.StoryTemplate != null && generatedStory.StoryTemplate.TotalPageNumbers > 0)
            {
                generatedStory.LastPageRead = generatedStory.StoryTemplate.TotalPageNumbers;
            }
            generatedStory.UpdatedAt = DateTime.UtcNow.AddHours(7);
            _unitOfWork.Repository<GeneratedStory>().Update(generatedStory);
        }

        if (!wasAlreadyCompleted)
        {
            await UpdateDailyUsageAsync(readingLog.ChildProfileId, addedDurationSeconds: 0, addedStoryReadCount: 1);
        }

        await _unitOfWork.CompleteAsync();

        return MapToReadingLogDto(readingLog, generatedStory);
    }

    public async Task<IEnumerable<CompletedReadingLogDto>> GetCompletedStoriesByChildAsync(Guid childProfileId)
    {
        var childProfile = await _unitOfWork.ChildProfileRepository.GetByIdAsync(childProfileId);
        if (childProfile == null)
        {
            throw new KeyNotFoundException($"Hồ sơ bé với Id {childProfileId} không tồn tại.");
        }

        if (Guid.TryParse(_currentUserService.UserId, out var loggedInUserId))
        {
            var currentChild = await _unitOfWork.ChildProfileRepository.GetByUserIdAsync(loggedInUserId);
            if (currentChild != null)
            {
                if (currentChild.Id != childProfileId)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền xem danh sách truyện đã đọc của bé khác!");
                }
            }
            else
            {
                bool isParentOfChild = childProfile.ParentProfileId == loggedInUserId;
                if (!isParentOfChild)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền xem danh sách truyện đã đọc của bé này!");
                }
            }
        }

        var completedLogs = await _unitOfWork.ReadingLogRepository.GetCompletedByChildProfileIdAsync(childProfileId);
        var result = new List<CompletedReadingLogDto>();

        foreach (var log in completedLogs)
        {
            var generatedStory = await _unitOfWork.ReadingLogRepository.GetGeneratedStoryWithTemplateAsync(log.GeneratedStoryId);
            var dto = _mapper.Map<CompletedReadingLogDto>(log);
            dto.CompletedAt = log.UpdatedAt ?? log.CreatedAt;

            if (generatedStory != null)
            {
                dto.StoryTemplateId = generatedStory.StoryTemplateId;
                dto.LastPageRead = generatedStory.LastPageRead;
                dto.IsFavorite = generatedStory.IsFavorite;

                if (generatedStory.StoryTemplate != null)
                {
                    dto.StoryTitle = generatedStory.StoryTemplate.Title;
                    dto.Description = generatedStory.StoryTemplate.Description;
                    dto.CoverImageUrl = generatedStory.StoryTemplate.CoverImageUrl;
                    dto.TotalPageNumbers = generatedStory.StoryTemplate.TotalPageNumbers;
                }
            }

            result.Add(dto);
        }

        return result;
    }

    private async Task UpdateDailyUsageAsync(Guid childProfileId, int addedDurationSeconds, int addedStoryReadCount)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        var dailyLog = await _unitOfWork.ReadingLogRepository.GetDailyUsageLogAsync(childProfileId, today);

        if (dailyLog == null)
        {
            dailyLog = new ChildDailyUsageLog
            {
                Id = Guid.NewGuid(),
                ChildProfileId = childProfileId,
                LogDate = today,
                DurationSecond = addedDurationSeconds,
                StoryReadCount = addedStoryReadCount,
                CharacterCreatedCount = 0,
                CreatedAt = DateTime.UtcNow.AddHours(7)
            };
            await _unitOfWork.ReadingLogRepository.AddDailyUsageLogAsync(dailyLog);
        }
        else
        {
            dailyLog.DurationSecond += addedDurationSeconds;
            dailyLog.StoryReadCount += addedStoryReadCount;
            dailyLog.UpdatedAt = DateTime.UtcNow.AddHours(7);
            _unitOfWork.ReadingLogRepository.UpdateDailyUsageLog(dailyLog);
        }
    }

    private ReadingLogDto MapToReadingLogDto(ReadingLog readingLog, GeneratedStory? generatedStory)
    {
        var dto = _mapper.Map<ReadingLogDto>(readingLog);
        if (generatedStory != null)
        {
            dto.StoryTemplateId = generatedStory.StoryTemplateId;
            dto.CurrentPage = generatedStory.LastPageRead;

            if (generatedStory.StoryTemplate != null)
            {
                dto.StoryTitle = generatedStory.StoryTemplate.Title;
                dto.CoverImageUrl = generatedStory.StoryTemplate.CoverImageUrl;
                dto.TotalPageNumbers = generatedStory.StoryTemplate.TotalPageNumbers;
            }
        }
        return dto;
    }
}

