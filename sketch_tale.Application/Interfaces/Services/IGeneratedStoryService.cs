using sketch_tale.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Services
{
    public interface IGeneratedStoryService
    {
        Task<GeneratedStoryDto> GenerateStoryAsync(GenerateStoryRequestDto request);
        Task<GeneratedStoryDetailsDto> GetGeneratedStoryByIdAsync(Guid id);
        Task<List<GeneratedStoryDetailsDto>> GetGeneratedStoriesByChildProfileIdAsync(Guid childProfileId);
    }
}
