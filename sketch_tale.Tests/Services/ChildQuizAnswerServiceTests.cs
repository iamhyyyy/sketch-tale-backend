using AutoMapper;
using NSubstitute;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Application.Mappings;
using sketch_tale.Application.Services;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace sketch_tale.Tests.Services;

public class ChildQuizAnswerServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ChildQuizAnswerService _service;

    public ChildQuizAnswerServiceTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUserService = Substitute.For<ICurrentUserService>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _service = new ChildQuizAnswerService(_unitOfWork, _mapper, _currentUserService);
    }

    [Fact]
    public async Task SubmitAnswerAsync_WhenValidAndCorrect_ReturnsCorrectResultAnd50PercentProgress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var childProfileId = Guid.NewGuid();
        var readingLogId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var generatedStoryId = Guid.NewGuid();
        var storyTemplateId = Guid.NewGuid();

        _currentUserService.UserId.Returns(userId.ToString());

        var childProfile = new ChildProfile { Id = childProfileId, UserId = userId };
        _unitOfWork.ChildProfileRepository.GetByUserIdAsync(userId).Returns(childProfile);

        var readingLog = new ReadingLog
        {
            Id = readingLogId,
            ChildProfileId = childProfileId,
            GeneratedStoryId = generatedStoryId,
            IsCompleted = false
        };
        _unitOfWork.ReadingLogRepository.GetByIdAsync(readingLogId).Returns(readingLog);

        var quiz = new StoryQuizTemplate
        {
            Id = quizId,
            StoryTemplateId = storyTemplateId,
            CorrectOptionIndex = 1,
            Status = CommonStatus.Active
        };
        _unitOfWork.StoryQuizTemplateRepository.GetByIdAsync(quizId).Returns(quiz);

        var generatedStory = new GeneratedStory
        {
            Id = generatedStoryId,
            StoryTemplateId = storyTemplateId
        };
        _unitOfWork.Repository<GeneratedStory>().GetByIdAsync(generatedStoryId).Returns(generatedStory);

        // Giả sử truyện có 2 câu hỏi, bé đang nộp câu đầu tiên
        _unitOfWork.StoryQuizTemplateRepository.GetQuizzesByStoryIdAsync(storyTemplateId)
            .Returns(new List<StoryQuizTemplate>
            {
                quiz,
                new StoryQuizTemplate { Id = Guid.NewGuid(), Status = CommonStatus.Active }
            });

        _unitOfWork.ChildQuizAnswerRepository.GetAnswerAsync(readingLogId, quizId)
            .Returns((ChildQuizAnswer?)null);

        _unitOfWork.ChildQuizAnswerRepository.GetAnswersByReadingLogIdAsync(readingLogId)
            .Returns(new List<ChildQuizAnswer>
            {
                new ChildQuizAnswer { Id = Guid.NewGuid(), ReadingLogId = readingLogId, IsCorrect = true }
            });

        var dto = new CreateChildQuizAnswerDto
        {
            ReadingLogId = readingLogId,
            StoryQuizTemplateId = quizId,
            SelectedOptionIndex = 1, // Chọn đúng
            ResponseTimeSeconds = 5
        };

        // Act
        var result = await _service.SubmitAnswerAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsCorrect);
        Assert.Equal(10, result.QuizScore);
        Assert.Equal(1, result.CorrectOptionIndex);
        Assert.Equal(50.0, result.CompletionPercentage); // 1/2 câu = 50%
        Assert.False(result.IsQuizCompleted);
        Assert.Equal(0, result.Stars); // Chưa làm xong bài -> 0 sao
        await _unitOfWork.Received(1).CompleteAsync();
    }

    [Fact]
    public async Task SubmitAnswerAsync_WhenDifferentChildProfile_ThrowsUnauthorizedAccessException()
    {
        // Arrange: Bé A đang đăng nhập nhưng cố tình nộp bài cho ReadingLog của Bé B
        var loggedInUserId = Guid.NewGuid();
        var loggedInChildId = Guid.NewGuid();
        var differentChildId = Guid.NewGuid();
        var readingLogId = Guid.NewGuid();

        _currentUserService.UserId.Returns(loggedInUserId.ToString());

        var childA = new ChildProfile { Id = loggedInChildId, UserId = loggedInUserId };
        _unitOfWork.ChildProfileRepository.GetByUserIdAsync(loggedInUserId).Returns(childA);

        var readingLogOfChildB = new ReadingLog
        {
            Id = readingLogId,
            ChildProfileId = differentChildId // Của bé khác!
        };
        _unitOfWork.ReadingLogRepository.GetByIdAsync(readingLogId).Returns(readingLogOfChildB);

        var dto = new CreateChildQuizAnswerDto
        {
            ReadingLogId = readingLogId,
            StoryQuizTemplateId = Guid.NewGuid(),
            SelectedOptionIndex = 0
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.SubmitAnswerAsync(dto));
        Assert.Contains("chính mình", ex.Message);
    }

    [Fact]
    public async Task SubmitAnswerAsync_WhenLastQuestionAndAllCorrect_CompletesReadingLogAndAwards3Stars()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var childProfileId = Guid.NewGuid();
        var readingLogId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var generatedStoryId = Guid.NewGuid();
        var storyTemplateId = Guid.NewGuid();

        _currentUserService.UserId.Returns(userId.ToString());
        _unitOfWork.ChildProfileRepository.GetByUserIdAsync(userId)
            .Returns(new ChildProfile { Id = childProfileId, UserId = userId });

        var readingLog = new ReadingLog
        {
            Id = readingLogId,
            ChildProfileId = childProfileId,
            GeneratedStoryId = generatedStoryId,
            IsCompleted = false
        };
        _unitOfWork.ReadingLogRepository.GetByIdAsync(readingLogId).Returns(readingLog);

        var quiz = new StoryQuizTemplate
        {
            Id = quizId,
            StoryTemplateId = storyTemplateId,
            CorrectOptionIndex = 2,
            Status = CommonStatus.Active
        };
        _unitOfWork.StoryQuizTemplateRepository.GetByIdAsync(quizId).Returns(quiz);

        _unitOfWork.Repository<GeneratedStory>().GetByIdAsync(generatedStoryId)
            .Returns(new GeneratedStory { Id = generatedStoryId, StoryTemplateId = storyTemplateId });

        // Tổng cộng có 2 câu hỏi
        _unitOfWork.StoryQuizTemplateRepository.GetQuizzesByStoryIdAsync(storyTemplateId)
            .Returns(new List<StoryQuizTemplate> { quiz, new StoryQuizTemplate { Id = Guid.NewGuid() } });

        // Bé đã trả lời đủ cả 2 câu và cả 2 câu đều đúng 100%
        _unitOfWork.ChildQuizAnswerRepository.GetAnswersByReadingLogIdAsync(readingLogId)
            .Returns(new List<ChildQuizAnswer>
            {
                new ChildQuizAnswer { Id = Guid.NewGuid(), ReadingLogId = readingLogId, IsCorrect = true },
                new ChildQuizAnswer { Id = Guid.NewGuid(), ReadingLogId = readingLogId, IsCorrect = true }
            });

        var dto = new CreateChildQuizAnswerDto
        {
            ReadingLogId = readingLogId,
            StoryQuizTemplateId = quizId,
            SelectedOptionIndex = 2
        };

        // Act
        var result = await _service.SubmitAnswerAsync(dto);

        // Assert
        Assert.True(result.IsQuizCompleted);
        Assert.Equal(100.0, result.CompletionPercentage);
        Assert.Equal(3, result.Stars); // Đúng full bài -> 3 sao!
        Assert.True(readingLog.IsCompleted); // Đã tự động cập nhật ReadingLog hoàn thành
        Assert.Contains("3 sao", result.Message);
    }

    [Fact]
    public async Task GetQuizAnswersByReadingLogIdAsync_WhenIncomplete_ReturnsZeroStarsAndProgress()
    {
        // Arrange
        var readingLogId = Guid.NewGuid();
        var childProfileId = Guid.NewGuid();
        var generatedStoryId = Guid.NewGuid();
        var storyTemplateId = Guid.NewGuid();

        var readingLog = new ReadingLog
        {
            Id = readingLogId,
            ChildProfileId = childProfileId,
            GeneratedStoryId = generatedStoryId
        };
        _unitOfWork.ReadingLogRepository.GetByIdAsync(readingLogId).Returns(readingLog);

        _unitOfWork.Repository<GeneratedStory>().GetByIdAsync(generatedStoryId)
            .Returns(new GeneratedStory { Id = generatedStoryId, StoryTemplateId = storyTemplateId });

        // Truyện có 4 câu
        _unitOfWork.StoryQuizTemplateRepository.GetQuizzesByStoryIdAsync(storyTemplateId)
            .Returns(new List<StoryQuizTemplate>
            {
                new StoryQuizTemplate { Id = Guid.NewGuid() },
                new StoryQuizTemplate { Id = Guid.NewGuid() },
                new StoryQuizTemplate { Id = Guid.NewGuid() },
                new StoryQuizTemplate { Id = Guid.NewGuid() }
            });

        // Bé mới làm 2 câu (out app dở dang)
        var quizTemplate = new StoryQuizTemplate
        {
            QuestionText = "1 + 1 = ?",
            OptionsJson = "[\"1\", \"2\", \"3\", \"4\"]",
            CorrectOptionIndex = 1
        };

        _unitOfWork.ChildQuizAnswerRepository.GetAnswersByReadingLogIdAsync(readingLogId)
            .Returns(new List<ChildQuizAnswer>
            {
                new ChildQuizAnswer
                {
                    Id = Guid.NewGuid(),
                    ReadingLogId = readingLogId,
                    IsCorrect = true,
                    SelectedOptionIndex = 1,
                    StoryQuizTemplate = quizTemplate
                },
                new ChildQuizAnswer
                {
                    Id = Guid.NewGuid(),
                    ReadingLogId = readingLogId,
                    IsCorrect = false,
                    SelectedOptionIndex = 0,
                    StoryQuizTemplate = quizTemplate
                }
            });

        // Act
        var result = await _service.GetQuizAnswersByReadingLogIdAsync(readingLogId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.TotalQuestions);
        Assert.Equal(2, result.TotalAnswered);
        Assert.Equal(1, result.TotalCorrectAnswers);
        Assert.Equal(50.0, result.CompletionPercentage); // 2/4 = 50%
        Assert.Equal(50.0, result.AccuracyPercentage);   // 1/2 = 50%
        Assert.False(result.IsQuizCompleted);
        Assert.Equal(0, result.Stars); // Đang làm dở -> 0 sao!
        Assert.Equal(2, result.Answers.Count);
        Assert.Equal(4, result.Answers[0].Options.Count); // Đã deserialize thành 4 options
    }

    [Fact]
    public async Task GetQuizAnswersByReadingLogIdAsync_WhenCompleteWith75PercentAccuracy_Returns2Stars()
    {
        // Arrange
        var readingLogId = Guid.NewGuid();
        var childProfileId = Guid.NewGuid();
        var generatedStoryId = Guid.NewGuid();
        var storyTemplateId = Guid.NewGuid();

        var readingLog = new ReadingLog
        {
            Id = readingLogId,
            ChildProfileId = childProfileId,
            GeneratedStoryId = generatedStoryId
        };
        _unitOfWork.ReadingLogRepository.GetByIdAsync(readingLogId).Returns(readingLog);

        _unitOfWork.Repository<GeneratedStory>().GetByIdAsync(generatedStoryId)
            .Returns(new GeneratedStory { Id = generatedStoryId, StoryTemplateId = storyTemplateId });

        // Truyện có 4 câu
        _unitOfWork.StoryQuizTemplateRepository.GetQuizzesByStoryIdAsync(storyTemplateId)
            .Returns(new List<StoryQuizTemplate>
            {
                new StoryQuizTemplate { Id = Guid.NewGuid() },
                new StoryQuizTemplate { Id = Guid.NewGuid() },
                new StoryQuizTemplate { Id = Guid.NewGuid() },
                new StoryQuizTemplate { Id = Guid.NewGuid() }
            });

        // Bé đã làm đủ 4 câu, đúng 3 câu (75%)
        _unitOfWork.ChildQuizAnswerRepository.GetAnswersByReadingLogIdAsync(readingLogId)
            .Returns(new List<ChildQuizAnswer>
            {
                new ChildQuizAnswer { Id = Guid.NewGuid(), ReadingLogId = readingLogId, IsCorrect = true },
                new ChildQuizAnswer { Id = Guid.NewGuid(), ReadingLogId = readingLogId, IsCorrect = true },
                new ChildQuizAnswer { Id = Guid.NewGuid(), ReadingLogId = readingLogId, IsCorrect = true },
                new ChildQuizAnswer { Id = Guid.NewGuid(), ReadingLogId = readingLogId, IsCorrect = false }
            });

        // Act
        var result = await _service.GetQuizAnswersByReadingLogIdAsync(readingLogId);

        // Assert
        Assert.True(result.IsQuizCompleted);
        Assert.Equal(100.0, result.CompletionPercentage);
        Assert.Equal(75.0, result.AccuracyPercentage);
        Assert.Equal(2, result.Stars); // 75% đúng (trong khoảng 60% - 90%) -> 2 sao!
    }

    [Fact]
    public async Task GetQuizAnswersByChildIdAsync_ReturnsAllReadingLogSummaries_ForValidChild()
    {
        // Arrange
        var childProfileId = Guid.NewGuid();
        var readingLogId1 = Guid.NewGuid();
        var readingLogId2 = Guid.NewGuid();
        var generatedStoryId = Guid.NewGuid();
        var storyTemplateId = Guid.NewGuid();

        // Be ton tai
        _unitOfWork.ChildProfileRepository.GetByIdAsync(childProfileId)
            .Returns(new ChildProfile { Id = childProfileId });

        // Khong co token be -> quyen cha/me, bo qua kiem tra userId
        _currentUserService.UserId.Returns((string)null!);

        var quizTemplate = new StoryQuizTemplate
        {
            QuestionText = "2 + 2 = ?",
            OptionsJson = "[\"3\", \"4\", \"5\"]",
            CorrectOptionIndex = 1
        };

        var readingLog1 = new ReadingLog
        {
            Id = readingLogId1,
            ChildProfileId = childProfileId,
            GeneratedStoryId = generatedStoryId
        };
        var readingLog2 = new ReadingLog
        {
            Id = readingLogId2,
            ChildProfileId = childProfileId,
            GeneratedStoryId = generatedStoryId
        };

        // Be da tra loi 2 cau o phien 1, 1 cau o phien 2
        _unitOfWork.ChildQuizAnswerRepository.GetAnswersByChildProfileIdAsync(childProfileId)
            .Returns(new List<ChildQuizAnswer>
            {
                new ChildQuizAnswer { Id = Guid.NewGuid(), ReadingLogId = readingLogId1, ChildProfileId = childProfileId, IsCorrect = true,  StoryQuizTemplate = quizTemplate, ReadingLog = readingLog1 },
                new ChildQuizAnswer { Id = Guid.NewGuid(), ReadingLogId = readingLogId1, ChildProfileId = childProfileId, IsCorrect = false, StoryQuizTemplate = quizTemplate, ReadingLog = readingLog1 },
                new ChildQuizAnswer { Id = Guid.NewGuid(), ReadingLogId = readingLogId2, ChildProfileId = childProfileId, IsCorrect = true,  StoryQuizTemplate = quizTemplate, ReadingLog = readingLog2 }
            });

        // Moi truyen co 2 cau hoi
        _unitOfWork.Repository<GeneratedStory>().GetByIdAsync(generatedStoryId)
            .Returns(new GeneratedStory { Id = generatedStoryId, StoryTemplateId = storyTemplateId });

        _unitOfWork.StoryQuizTemplateRepository.GetQuizzesByStoryIdAsync(storyTemplateId)
            .Returns(new List<StoryQuizTemplate>
            {
                new StoryQuizTemplate { Id = Guid.NewGuid() },
                new StoryQuizTemplate { Id = Guid.NewGuid() }
            });

        // Act
        var results = (await _service.GetQuizAnswersByChildIdAsync(childProfileId)).ToList();

        // Assert
        Assert.Equal(2, results.Count); // 2 phien doc

        var summary1 = results.First(r => r.ReadingLogId == readingLogId1);
        Assert.Equal(2, summary1.TotalAnswered);
        Assert.Equal(1, summary1.TotalCorrectAnswers);
        Assert.Equal(100.0, summary1.CompletionPercentage); // 2/2 cau -> 100%
        Assert.True(summary1.IsQuizCompleted);
        Assert.Equal(2, summary1.Stars); // 50% dung (>= 50%) -> 2 sao

        var summary2 = results.First(r => r.ReadingLogId == readingLogId2);
        Assert.Equal(1, summary2.TotalAnswered);
        Assert.Equal(50.0, summary2.CompletionPercentage); // 1/2 cau -> 50%
        Assert.False(summary2.IsQuizCompleted);
        Assert.Equal(0, summary2.Stars); // Chua lam het -> 0 sao
    }

    [Fact]
    public async Task GetQuizAnswersByChildIdAsync_WhenDifferentChild_ThrowsUnauthorizedAccessException()
    {
        // Arrange: Be A dang dang nhap nhung co tinh xem ket qua cua Be B
        var loggedInUserId = Guid.NewGuid();
        var loggedInChildId = Guid.NewGuid();
        var targetChildProfileId = Guid.NewGuid(); // Be B

        _currentUserService.UserId.Returns(loggedInUserId.ToString());
        _unitOfWork.ChildProfileRepository.GetByUserIdAsync(loggedInUserId)
            .Returns(new ChildProfile { Id = loggedInChildId, UserId = loggedInUserId });

        // Be B ton tai
        _unitOfWork.ChildProfileRepository.GetByIdAsync(targetChildProfileId)
            .Returns(new ChildProfile { Id = targetChildProfileId });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.GetQuizAnswersByChildIdAsync(targetChildProfileId));
        Assert.Contains("quyen", ex.Message);
    }
}