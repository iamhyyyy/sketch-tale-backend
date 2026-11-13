using sketch_tale.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Services;

public interface IReadingLogService
{
    Task<ReadingLogDto> StartReadingSessionAsync(StartReadingLogDto dto);
    Task<ReadingLogDto> UpdateReadingProgressAsync(Guid readingLogId, UpdateReadingProgressDto dto);
    Task<ReadingLogDto> CompleteReadingSessionAsync(Guid readingLogId);
    Task<IEnumerable<CompletedReadingLogDto>> GetCompletedStoriesByChildAsync(Guid childProfileId);
}

