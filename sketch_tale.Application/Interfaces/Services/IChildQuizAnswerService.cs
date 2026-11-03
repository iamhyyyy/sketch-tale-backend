using sketch_tale.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Services;

public interface IChildQuizAnswerService
{
    Task<SubmitQuizAnswerResponseDto> SubmitAnswerAsync(CreateChildQuizAnswerDto dto);
    Task<IEnumerable<ReadingLogQuizSummaryDto>> GetQuizAnswersByChildIdAsync(Guid childId);
    Task<ReadingLogQuizSummaryDto> GetQuizAnswersByReadingLogIdAsync(Guid readingLogId);
}
