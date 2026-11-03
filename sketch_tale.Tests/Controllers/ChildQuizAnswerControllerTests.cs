//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using NSubstitute;
//using NSubstitute.ExceptionExtensions;
//using sketch_tale.API.Controllers;
//using sketch_tale.Application.DTOs;
//using sketch_tale.Application.Interfaces.Services;
//using System;
//using System.Threading.Tasks;
//using Xunit;

//namespace sketch_tale.Tests.Controllers;

//public class ChildQuizAnswerControllerTests
//{
//    private readonly IChildQuizAnswerService _service;
//    private readonly ChildQuizAnswerController _controller;

//    public ChildQuizAnswerControllerTests()
//    {
//        _service = Substitute.For<IChildQuizAnswerService>();
//        _controller = new ChildQuizAnswerController(_service);
//    }

//    [Fact]
//    public async Task SubmitAnswer_WhenValid_ReturnsOkWithResponse()
//    {
//        // Arrange
//        var dto = new CreateChildQuizAnswerDto
//        {
//            ReadingLogId = Guid.NewGuid(),
//            StoryQuizTemplateId = Guid.NewGuid(),
//            SelectedOptionIndex = 1
//        };

//        var expectedResponse = new SubmitQuizAnswerResponseDto
//        {
//            IsCorrect = true,
//            QuizScore = 10,
//            CompletionPercentage = 50.0,
//            Stars = 0
//        };

//        _service.SubmitAnswerAsync(dto).Returns(expectedResponse);

//        // Act
//        var actionResult = await _controller.SubmitAnswer(dto);

//        // Assert
//        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
//        var returnDto = Assert.IsType<SubmitQuizAnswerResponseDto>(okResult.Value);
//        Assert.True(returnDto.IsCorrect);
//        Assert.Equal(50.0, returnDto.CompletionPercentage);
//    }

//    [Fact]
//    public async Task SubmitAnswer_WhenUnauthorizedAccess_Returns403Forbidden()
//    {
//        // Arrange: Bé cố tình nộp bài cho ReadingLog của người khác
//        var dto = new CreateChildQuizAnswerDto
//        {
//            ReadingLogId = Guid.NewGuid(),
//            StoryQuizTemplateId = Guid.NewGuid(),
//            SelectedOptionIndex = 0
//        };

//        _service.SubmitAnswerAsync(dto).Throws(new UnauthorizedAccessException("Không có quyền!"));

//        // Act
//        var actionResult = await _controller.SubmitAnswer(dto);

//        // Assert
//        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
//        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
//    }

//    [Fact]
//    public async Task SubmitAnswer_WhenNotFound_Returns404NotFound()
//    {
//        // Arrange: ReadingLogId hoặc QuizTemplateId không tồn tại
//        var dto = new CreateChildQuizAnswerDto
//        {
//            ReadingLogId = Guid.NewGuid(),
//            StoryQuizTemplateId = Guid.NewGuid()
//        };

//        _service.SubmitAnswerAsync(dto).Throws(new KeyNotFoundException("Không tìm thấy"));

//        // Act
//        var actionResult = await _controller.SubmitAnswer(dto);

//        // Assert
//        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
//    }

//    [Fact]
//    public async Task GetQuizAnswersByReadingLog_WhenFound_ReturnsOkWithSummary()
//    {
//        // Arrange
//        var readingLogId = Guid.NewGuid();
//        var summaryDto = new ReadingLogQuizSummaryDto
//        {
//            ReadingLogId = readingLogId,
//            TotalQuestions = 4,
//            TotalAnswered = 4,
//            TotalCorrectAnswers = 4,
//            CompletionPercentage = 100.0,
//            AccuracyPercentage = 100.0,
//            IsQuizCompleted = true,
//            Stars = 3
//        };

//        _service.GetQuizAnswersByReadingLogIdAsync(readingLogId).Returns(summaryDto);

//        // Act
//        var actionResult = await _controller.GetQuizAnswersByReadingLog(readingLogId);

//        // Assert
//        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
//        var returnDto = Assert.IsType<ReadingLogQuizSummaryDto>(okResult.Value);
//        Assert.Equal(3, returnDto.Stars);
//        Assert.Equal(100.0, returnDto.CompletionPercentage);
//    }

//    [Fact]
//    public async Task GetQuizAnswersByReadingLog_WhenNotFound_Returns404NotFound()
//    {
//        // Arrange
//        var readingLogId = Guid.NewGuid();
//        _service.GetQuizAnswersByReadingLogIdAsync(readingLogId).Throws(new KeyNotFoundException("Không tồn tại"));

//        // Act
//        var actionResult = await _controller.GetQuizAnswersByReadingLog(readingLogId);

//        // Assert
//        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
//    }
//}
