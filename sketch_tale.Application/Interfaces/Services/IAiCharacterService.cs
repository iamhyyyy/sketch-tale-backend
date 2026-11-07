using sketch_tale.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Services
{
    public interface IAiCharacterService
    {
        Task<AiAnalyzeResultDto> AnalyzeImageAsync(string imageUrl, IReadOnlyCollection<AiAllowedCharTypeDto> allowedCharTypes);

        Task<string> GenerateImageAsync(string finalPrompt, string originalImageUrl);
    }

}
