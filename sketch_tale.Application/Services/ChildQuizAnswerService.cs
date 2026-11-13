using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace sketch_tale.Application.Services;

public class ChildQuizAnswerService : IChildQuizAnswerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public ChildQuizAnswerService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<SubmitQuizAnswerResponseDto> SubmitAnswerAsync(CreateChildQuizAnswerDto dto)
    {
        var readingLog = await _unitOfWork.ReadingLogRepository.GetByIdAsync(dto.ReadingLogId);
        if (readingLog == null)
        {
            throw new KeyNotFoundException($"Phiên đọc truyện với Id {dto.ReadingLogId} không tồn tại.");
        }

        Guid childProfileId = Guid.Empty;

        // 1. Xác thực danh tính người đang làm bài từ Token đăng nhập
        if (Guid.TryParse(_currentUserService.UserId, out var loggedInUserId))
        {
            var currentChild = await _unitOfWork.ChildProfileRepository.GetByUserIdAsync(loggedInUserId);
            if (currentChild != null)
            {
                childProfileId = currentChild.Id;

                // Kiểm tra xem bé đang đăng nhập có đúng là người sở hữu phiên đọc này không
                if (readingLog.ChildProfileId != currentChild.Id)
                {
                    throw new UnauthorizedAccessException("Bé chỉ có thể làm bài kiểm tra trong phiên đọc truyện của chính mình!");
                }
            }
        }

        // Fallback nếu không xác định được từ token (hoặc khi dev test trực tiếp qua DTO)
        if (childProfileId == Guid.Empty)
        {
            childProfileId = dto.ChildProfileId.HasValue && dto.ChildProfileId.Value != Guid.Empty
                ? dto.ChildProfileId.Value
                : readingLog.ChildProfileId;
        }

        var quiz = await _unitOfWork.StoryQuizTemplateRepository.GetByIdAsync(dto.StoryQuizTemplateId);
        if (quiz == null || quiz.Status == CommonStatus.Inactive)
        {
            throw new KeyNotFoundException($"Câu hỏi trắc nghiệm với Id {dto.StoryQuizTemplateId} không tồn tại hoặc đã ngừng hoạt động.");
        }

        if (dto.SelectedOptionIndex < 0)
        {
            throw new ArgumentException("Đáp án lựa chọn không hợp lệ.");
        }

        bool isCorrect = (dto.SelectedOptionIndex == quiz.CorrectOptionIndex);
        int score = isCorrect ? 10 : 0;

        var existingAnswer = await _unitOfWork.ChildQuizAnswerRepository.GetAnswerAsync(dto.ReadingLogId, dto.StoryQuizTemplateId);
        ChildQuizAnswer answerEntity;

        if (existingAnswer != null)
        {
            existingAnswer.SelectedOptionIndex = dto.SelectedOptionIndex;
            existingAnswer.IsCorrect = isCorrect;
            existingAnswer.QuizScore = score;
            existingAnswer.ResponseTimeSeconds = dto.ResponseTimeSeconds;
            existingAnswer.UpdatedAt = DateTime.UtcNow.AddHours(7);

            _unitOfWork.ChildQuizAnswerRepository.Update(existingAnswer);
            answerEntity = existingAnswer;
        }
        else
        {
            answerEntity = new ChildQuizAnswer
            {
                Id = Guid.NewGuid(),
                ChildProfileId = childProfileId,
                ReadingLogId = dto.ReadingLogId,
                StoryQuizTemplateId = dto.StoryQuizTemplateId,
                SelectedOptionIndex = dto.SelectedOptionIndex,
                IsCorrect = isCorrect,
                QuizScore = score,
                ResponseTimeSeconds = dto.ResponseTimeSeconds,
                CreatedAt = DateTime.UtcNow.AddHours(7)
            };

            await _unitOfWork.ChildQuizAnswerRepository.AddAsync(answerEntity);
        }

        await _unitOfWork.CompleteAsync();

        // Tính toán tiến độ làm bài quiz sau khi nộp câu này
        int totalQuestions = 0;
        var generatedStory = await _unitOfWork.Repository<GeneratedStory>().GetByIdAsync(readingLog.GeneratedStoryId);
        if (generatedStory != null)
        {
            var quizzes = await _unitOfWork.StoryQuizTemplateRepository.GetQuizzesByStoryIdAsync(generatedStory.StoryTemplateId);
            totalQuestions = quizzes.Count();
        }

        var allAnswers = await _unitOfWork.ChildQuizAnswerRepository.GetAnswersByReadingLogIdAsync(dto.ReadingLogId);
        int totalAnswered = allAnswers.Count();
        if (totalQuestions == 0)
        {
            totalQuestions = totalAnswered;
        }

        double completionPercentage = totalQuestions > 0
            ? Math.Round(((double)totalAnswered / totalQuestions) * 100, 1)
            : 100.0;
        bool isQuizCompleted = totalQuestions > 0 && totalAnswered >= totalQuestions;

        int totalCorrect = allAnswers.Count(a => a.IsCorrect);
        int stars = CalculateStars(isQuizCompleted, totalCorrect, totalQuestions);

        var response = _mapper.Map<SubmitQuizAnswerResponseDto>(answerEntity);
        response.CorrectOptionIndex = quiz.CorrectOptionIndex;
        response.CurrentAnsweredCount = totalAnswered;
        response.TotalQuestions = totalQuestions;
        response.CompletionPercentage = completionPercentage;
        response.IsQuizCompleted = isQuizCompleted;
        response.Stars = stars;

        if (isQuizCompleted)
        {
            if (stars == 3)
                response.Message = "Xuất sắc! Bé đã làm đúng tất cả các câu và đạt 3 sao! ⭐⭐⭐";
            else if (stars == 2)
                response.Message = "Bé làm giỏi lắm! Bé đã hoàn thành bài kiểm tra và đạt 2 sao! ⭐⭐";
            else
                response.Message = "Bé đã hoàn thành bài kiểm tra và nhận được 1 sao khích lệ! ⭐";
        }
        else
        {
            response.Message = isCorrect
                ? "Bé giỏi quá! Đáp án hoàn toàn chính xác! 🎉"
                : "Chưa chính xác rồi, bé hãy thử lại ở những câu hỏi tiếp theo nhé! 💪";
        }

        return response;
    }

    public async Task<ReadingLogQuizSummaryDto> GetQuizAnswersByReadingLogIdAsync(Guid readingLogId)
    {
        var readingLog = await _unitOfWork.ReadingLogRepository.GetByIdAsync(readingLogId);
        if (readingLog == null)
        {
            throw new KeyNotFoundException($"Phiên đọc truyện với Id {readingLogId} không tồn tại.");
        }

        // Kiểm tra quyền: nếu là tài khoản bé đang đăng nhập, chỉ được xem kết quả của chính mình
        if (Guid.TryParse(_currentUserService.UserId, out var loggedInUserId))
        {
            var currentChild = await _unitOfWork.ChildProfileRepository.GetByUserIdAsync(loggedInUserId);
            if (currentChild != null && readingLog.ChildProfileId != currentChild.Id)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem kết quả bài kiểm tra của bé khác!");
            }
        }

        var answers = await _unitOfWork.ChildQuizAnswerRepository.GetAnswersByReadingLogIdAsync(readingLogId);
        return await BuildReadingLogSummaryAsync(
            readingLogId,
            readingLog.ChildProfileId,
            readingLog.GeneratedStoryId,
            answers.ToList());
    }

    public async Task<IEnumerable<ReadingLogQuizSummaryDto>> GetQuizAnswersByChildIdAsync(Guid childProfileId)
    {
        // 1. Kiểm tra hồ sơ bé có tồn tại không
        var childProfile = await _unitOfWork.ChildProfileRepository.GetByIdAsync(childProfileId);
        if (childProfile == null)
        {
            throw new KeyNotFoundException($"Hồ sơ bé với Id {childProfileId} không tồn tại.");
        }

        // 2. Kiểm tra quyền truy cập
        if (Guid.TryParse(_currentUserService.UserId, out var loggedInUserId))
        {
            var currentChild = await _unitOfWork.ChildProfileRepository.GetByUserIdAsync(loggedInUserId);

            if (currentChild != null)
            {
                if (currentChild.Id != childProfileId)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền xem kết quả bài kiểm tra của bé khác!");
                }
            }
            else
            {
                bool isParentOfChild = childProfile.ParentProfileId == loggedInUserId;
                if (!isParentOfChild)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền xem kết quả bài kiểm tra của bé này!");
                }
            }
        }

        // 3. Lấy tất cả câu trả lời của bé, gom nhóm theo ReadingLogId
        var allAnswers = await _unitOfWork.ChildQuizAnswerRepository.GetAnswersByChildProfileIdAsync(childProfileId);
        var groupedByReadingLog = allAnswers
            .GroupBy(a => a.ReadingLogId)
            .ToList();

        var summaries = new List<ReadingLogQuizSummaryDto>();

        foreach (var group in groupedByReadingLog)
        {
            var readingLogId = group.Key;
            var answers = group.ToList();

            var readingLog = answers.First().ReadingLog
                             ?? await _unitOfWork.ReadingLogRepository.GetByIdAsync(readingLogId);
            if (readingLog == null) continue;

            var summary = await BuildReadingLogSummaryAsync(readingLogId, readingLog.ChildProfileId, readingLog.GeneratedStoryId, answers);
            summaries.Add(summary);
        }

        return summaries;
    }

    /// <summary>
    /// Đánh giá sao theo logic:
    /// - Chưa làm hết quiz: 0 sao (không hiển thị sao nào)
    /// - Làm hết full bài và đúng hết: 3 sao
    /// - Làm hết full bài và đúng 50% - dưới 100% (khoảng 60% - 90%): 2 sao
    /// - Làm hết full bài và đúng dưới 50%: 1 sao
    /// </summary>
    private static int CalculateStars(bool isQuizCompleted, int totalCorrect, int totalQuestions)
    {
        if (!isQuizCompleted || totalQuestions <= 0)
        {
            return 0;
        }

        if (totalCorrect == totalQuestions)
        {
            return 3;
        }

        double accuracyPercentage = Math.Round(((double)totalCorrect / totalQuestions) * 100, 1);
        if (accuracyPercentage >= 50.0)
        {
            return 2;
        }

        return 1;
    }

    /// <summary>
    /// Helper dùng chung để tính toán ReadingLogQuizSummaryDto từ danh sách câu trả lời của một phiên đọc.
    /// </summary>
    private async Task<ReadingLogQuizSummaryDto> BuildReadingLogSummaryAsync(
        Guid readingLogId,
        Guid childProfileId,
        Guid generatedStoryId,
        List<ChildQuizAnswer> answers)
    {
        int totalQuestions = 0;
        var generatedStory = await _unitOfWork.Repository<GeneratedStory>().GetByIdAsync(generatedStoryId);
        if (generatedStory != null)
        {
            var quizzes = await _unitOfWork.StoryQuizTemplateRepository.GetQuizzesByStoryIdAsync(generatedStory.StoryTemplateId);
            totalQuestions = quizzes.Count();
        }

        if (totalQuestions == 0)
            totalQuestions = answers.Count;

        int totalAnswered = answers.Count;
        int totalCorrect = answers.Count(a => a.IsCorrect);

        double completionPercentage = totalQuestions > 0
            ? Math.Round(((double)totalAnswered / totalQuestions) * 100, 1)
            : 0.0;
        double accuracyPercentage = totalAnswered > 0
            ? Math.Round(((double)totalCorrect / totalAnswered) * 100, 1)
            : 0.0;
        bool isQuizCompleted = totalQuestions > 0 && totalAnswered >= totalQuestions;

        int stars = CalculateStars(isQuizCompleted, totalCorrect, totalQuestions);

        var answersDto = answers.Select(a =>
        {
            var dto = _mapper.Map<ChildQuizAnswerDto>(a);
            if (a.StoryQuizTemplate != null)
            {
                dto.QuestionText = a.StoryQuizTemplate.QuestionText;
                dto.QuestionAudioUrl = a.StoryQuizTemplate.QuestionAudioUrl;
                dto.QuestionPicUrl = a.StoryQuizTemplate.QuestionPicUrl;
                dto.CorrectOptionIndex = a.StoryQuizTemplate.CorrectOptionIndex;

                if (!string.IsNullOrWhiteSpace(a.StoryQuizTemplate.OptionsJson))
                {
                    try
                    {
                        dto.Options = JsonSerializer.Deserialize<List<string>>(a.StoryQuizTemplate.OptionsJson) ?? new List<string>();
                    }
                    catch
                    {
                        dto.Options = new List<string>();
                    }
                }
            }
            return dto;
        }).ToList();

        return new ReadingLogQuizSummaryDto
        {
            ReadingLogId = readingLogId,
            ChildProfileId = childProfileId,
            TotalQuestions = totalQuestions,
            TotalAnswered = totalAnswered,
            TotalCorrectAnswers = totalCorrect,
            CompletionPercentage = completionPercentage,
            AccuracyPercentage = accuracyPercentage,
            IsQuizCompleted = isQuizCompleted,
            Stars = stars,
            Answers = answersDto
        };
    }
}
