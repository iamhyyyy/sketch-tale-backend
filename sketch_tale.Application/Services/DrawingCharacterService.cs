using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Services
{
    public class DrawingCharacterService : IDrawingCharacterService
    {
        private readonly IAiCharacterService _aiService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinary;

        public DrawingCharacterService(IAiCharacterService aiService, IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryService cloudinary)
        {
            _aiService = aiService;
            _unitOfWork = unitOfWork;
            _cloudinary = cloudinary;
            _mapper = mapper;
        }

        public async Task<AnalyzeDrawingResponse> AnalyzeAsync(
    AnalyzeDrawingRequest request)
        {
            // 1. Validate dữ liệu đầu vào
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.File == null || request.File.Length == 0)
                throw new ArgumentException("Vui lòng cung cấp hình vẽ.");

            if (request.ChildProfileId == Guid.Empty)
                throw new ArgumentException("ChildProfileId không hợp lệ.");

            // 2. Upload ảnh lên Cloudinary
            var originalUrl = await _cloudinary.UploadImageAsync(
                request.File,
                "ChildUpload"
            );

            // 3. Tạo Drawing
            var drawing = new Drawing
            {
                ChildProfileId = request.ChildProfileId,
                OriginalImageUrl = originalUrl,
                DrawingType = request.DrawingType
            };

            // 4. Lưu Drawing trước khi gọi AI
            await _unitOfWork.DrawingRepository.AddAsync(drawing);
            await _unitOfWork.CompleteAsync();

            // 5. Lấy danh sách CharType Active
            var activeCharTypes =
                await _unitOfWork.CharTypeRepository.GetActiveCharTypesAsync();

            if (activeCharTypes == null || !activeCharTypes.Any())
            {
                throw new InvalidOperationException(
                    "Hệ thống hiện không có CharType nào đang hoạt động."
                );
            }

            // 6. Mapping Entity thành DTO dành cho AI
            var allowedCharTypes =
                _mapper.Map<List<AiAllowedCharTypeDto>>(activeCharTypes);

            // 7. Gọi Gemini phân tích hình vẽ
            var aiResult = await _aiService.AnalyzeImageAsync(
                originalUrl,
                allowedCharTypes
            );

            if (aiResult == null)
            {
                throw new InvalidOperationException(
                    "AI không trả về kết quả phân tích."
                );
            }

            // 8. Validate CharType IDs do AI đề xuất
            var activeCharTypesById = activeCharTypes
                .ToDictionary(c => c.Id);

            var suggestedIds = aiResult.SuggestedCharTypeIds
                ?? new List<Guid>();

            // Chỉ giữ ID tồn tại và đang Active, loại bỏ ID trùng
            // Giữ nguyên thứ tự ưu tiên mà Gemini đề xuất
            var validSuggestedIds = suggestedIds
                .Where(id => activeCharTypesById.ContainsKey(id))
                .Distinct()
                .Take(3)
                .ToList();

            // 9. Lấy tối đa 3 CharType theo danh sách Gemini đề xuất
            var charTypesToReturn = validSuggestedIds
                .Select(id => activeCharTypesById[id])
                .ToList();

            // 10. Mapping sang DTO trả về Frontend
            var suggestedTypes = _mapper.Map<List<AiAllowedCharTypeDto>>(
                charTypesToReturn
            );

            // 11. Trả kết quả theo đúng TH1 (HappyCase) và TH2 (BadCase)
            return new AnalyzeDrawingResponse
            {
                DrawingId = drawing.Id,
                OriginalImageUrl = originalUrl,
                IsRecognized = aiResult.IsRecognized,
                StandardizedPrompt = aiResult.IsRecognized ? aiResult.Prompt : null,
                SuggestedCharTypes = suggestedTypes,
                DetectedTags = aiResult.IsRecognized ? (aiResult.DetectedTags ?? new List<string>()) : null
            };
        }

        public async Task<GenerateCharacterResponse> GenerateAsync(GenerateCharacterRequest request)
        {
            throw new NotImplementedException();
        }
    }
}