using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Services
{
    public class DrawingCharacterService : IDrawingCharacterService
    {
        private readonly IAiCharacterService _aiService;
        private readonly IDrawingRepository _drawingRepo;
        private readonly ICloudinaryService _cloudinary;

        public DrawingCharacterService(IAiCharacterService aiService, IDrawingRepository drawingRepo, ICloudinaryService cloudinary)
        {
            _aiService = aiService;
            _drawingRepo = drawingRepo;
            _cloudinary = cloudinary;
        }

        public async Task<AnalyzeDrawingResponse> AnalyzeAsync(AnalyzeDrawingRequest request)
        {
            var originalUrl = await _cloudinary.UploadImageAsync(request.File, "ChildUpload");

            var activeCharTypes = await _drawingRepo.GetActiveCharTypesAsync();

            var aiResult = await _aiService.AnalyzeImageAsync(originalUrl, activeCharTypes);
            var drawing = new Drawing { OriginalImageUrl = originalUrl, Status = aiResult.IsRecognized ? "Analyzed" : "Failed" };
            await _drawingRepo.AddDrawingAsync(drawing);

            return new AnalyzeDrawingResponse
            {
                DrawingId = drawingId,
                OriginalImageUrl = originalUrl,
                IsRecognized = aiResult.IsRecognized,
                StandardizedPrompt = aiResult.Prompt,
                SuggestedCharTypes = aiResult.IsRecognized
                    ? activeCharTypes.Where(c => aiResult.SuggestedCharTypeIds.Contains(c.Id)).ToList()
                    : activeCharTypes.Take(5).ToList()
            };
        }

        public async Task<GenerateCharacterResponse> GenerateAsync(GenerateCharacterRequest request)
        {

            var drawing = await _drawingRepo.GetByIdAsync(request.DrawingId);
            var charTypeName = await _drawingRepo.GetCharTypeNameAsync(request.CharTypeId);

            string finalPrompt = request.StandardizedPrompt;

            if (string.IsNullOrEmpty(finalPrompt))
            {
                var basePrompt = $"A cute 2D children's storybook illustration of a {charTypeName}";
                var ideaPart = string.IsNullOrEmpty(request.ChildAdditionalIdea) ? "" : $", {request.ChildAdditionalIdea}";
                var stylePart = ", preserving the child's hand-drawn color palette, isolated on a pure solid white background, vibrant colors, Pixar style, child-safe.";

                finalPrompt = basePrompt + ideaPart + stylePart;
            }

            var newImageUrl = await _aiService.GenerateImageAsync(finalPrompt, drawing.ImageUrl);

            return new GenerateCharacterResponse
            {
                AiCharacterGenerateId = Guid.NewGuid(),
                GenerateImageUrl = newImageUrl,
                AiPromptUsed = finalPrompt 
            };
        }
    }
}
