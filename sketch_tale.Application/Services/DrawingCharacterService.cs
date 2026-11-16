using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using System.Text.Json;

namespace sketch_tale.Application.Services
{
    public class DrawingCharacterService : IDrawingCharacterService
    {
        private readonly IAiCharacterService _aiService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinary;
        private readonly ICurrentUserService _currentUserService;

        public DrawingCharacterService(
            IAiCharacterService aiService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICloudinaryService cloudinary,
            ICurrentUserService currentUserService)
        {
            _aiService = aiService;
            _unitOfWork = unitOfWork;
            _cloudinary = cloudinary;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<AnalyzeDrawingResponse> AnalyzeAsync(AnalyzeDrawingRequest request)
        {
            // 1. Validate dữ liệu đầu vào & lấy ChildProfileId (từ Request hoặc từ JWT Token)
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.File == null || request.File.Length == 0)
                throw new ArgumentException("Vui lòng cung cấp hình vẽ.");

            var childProfileId = request.ChildProfileId;
            if (childProfileId == Guid.Empty && Guid.TryParse(_currentUserService.UserId, out var currentUserId))
            {
                childProfileId = currentUserId;
            }

            if (childProfileId == Guid.Empty)
                throw new ArgumentException("ChildProfileId không hợp lệ.");

            // 2. Kiểm tra hạn mức lượt tạo nhân vật trong ngày của bé & số dư Credit của Phụ huynh
            var childProfile = await _unitOfWork.ChildProfileRepository.GetByIdAsync(childProfileId);
            if (childProfile == null)
                throw new KeyNotFoundException("Không tìm thấy hồ sơ của bé (ChildProfile).");

            if (childProfile.RemainingCharacterLimit <= 0)
                throw new InvalidOperationException("Bé đã hết lượt tạo nhân vật trong ngày.");

            var parentProfile = await _unitOfWork.ParentProfileRepository.GetByIdAsync(childProfile.ParentProfileId);
            if (parentProfile == null)
                throw new KeyNotFoundException("Không tìm thấy hồ sơ phụ huynh (ParentProfile).");

            var classifyCost = await _unitOfWork.CreditCostRepository.GetCostByFeatureKeyAsync("ImageClassification", 30);
            var generateCost = await _unitOfWork.CreditCostRepository.GetCostByFeatureKeyAsync("CharacterGeneration", 150);
            var minimumRequiredCredit = classifyCost + generateCost;

            if (parentProfile.RemainingMonthlyCredit < minimumRequiredCredit)
            {
                throw new InvalidOperationException(
                    $"Số dư Credit của phụ huynh không đủ (cần tối thiểu {minimumRequiredCredit} credits, hiện còn {parentProfile.RemainingMonthlyCredit} credits)."
                );
            }

            // 3. Upload ảnh lên Cloudinary
            var originalUrl = await _cloudinary.UploadImageAsync(
                request.File,
                "ChildUpload"
            );

            // 4. Tạo và lưu dòng dữ liệu trong bảng Drawing
            var drawing = new Drawing
            {
                ChildProfileId = childProfileId,
                OriginalImageUrl = originalUrl,
                DrawingType = request.DrawingType
            };

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

            // 8. Trừ Credit phân loại tranh (-30 credits) & ghi CreditTransactionLog + AIUsageLog
            var creditBefore = parentProfile.RemainingMonthlyCredit;
            parentProfile.RemainingMonthlyCredit -= classifyCost;
            _unitOfWork.ParentProfileRepository.Update(parentProfile);

            await _unitOfWork.CreditTransactionLogRepository.AddAsync(new CreditTransactionLog
            {
                ParentProfileId = parentProfile.Id,
                ActionType = ActionType.Spend,
                FeatureKey = "ImageClassification",
                Amount = classifyCost,
                CreditBefore = creditBefore,
                CreditAfter = parentProfile.RemainingMonthlyCredit,
                Description = $"Phân tích hình vẽ của bé (DrawingId: {drawing.Id})"
            });

            await _unitOfWork.Repository<AIUsageLog>().AddAsync(new AIUsageLog
            {
                Provider = "GoogleGemini",
                ActionType = AIActionType.ClassifyImage,
                TokenUsed = 0,
                CostAmount = classifyCost
            });

            await _unitOfWork.CompleteAsync();

            // 9. Validate CharType IDs do AI đề xuất (chỉ giữ ID tồn tại và đang Active, tối đa 3)
            var activeCharTypesById = activeCharTypes.ToDictionary(c => c.Id);

            var suggestedIds = aiResult.SuggestedCharTypeIds ?? new List<Guid>();

            var validSuggestedIds = suggestedIds
                .Where(id => activeCharTypesById.ContainsKey(id))
                .Distinct()
                .Take(3)
                .ToList();

            var charTypesToReturn = validSuggestedIds
                .Select(id => activeCharTypesById[id])
                .ToList();

            var suggestedTypes = _mapper.Map<List<AiAllowedCharTypeDto>>(charTypesToReturn);

            // 10. Trả kết quả theo đúng TH1 (HappyCase) và TH2 (BadCase)
            return new AnalyzeDrawingResponse
            {
                DrawingId = drawing.Id,
                OriginalImageUrl = originalUrl,
                IsRecognized = aiResult.IsRecognized,
                StandardizedPrompt = aiResult.IsRecognized ? aiResult.Prompt : null,
                SuggestedCharTypes = suggestedTypes,
                DetectedTags = aiResult.IsRecognized ? (aiResult.DetectedTags ?? new List<string>()) : null,
                RemainingParentCredits = parentProfile.RemainingMonthlyCredit,
                RemainingDailyCharacterLimit = childProfile.RemainingCharacterLimit
            };
        }

        public async Task<GenerateCharacterResponse> GenerateAsync(GenerateCharacterRequest request)
        {
            // 1. Validate dữ liệu đầu vào
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.DrawingId == Guid.Empty)
                throw new ArgumentException("DrawingId không hợp lệ.");

            // Kiểm tra xem bé có chọn CharType hoặc nhập ý tưởng không
            var hasCharType = request.CharTypeId.HasValue && request.CharTypeId.Value != Guid.Empty;
            var hasAdditionalIdea = !string.IsNullOrWhiteSpace(request.ChildAdditionalIdea);

            if (!hasCharType && !hasAdditionalIdea)
            {
                throw new ArgumentException("Vui lòng chọn loài nhân vật hoặc nhập ý tưởng mô tả của bé.");
            }

            if (hasAdditionalIdea)
            {
                var ideaNormalizedForMod = request.ChildAdditionalIdea!
                    .Normalize(System.Text.NormalizationForm.FormC)
                    .Trim();
                var restrictedKeywords = await _unitOfWork.RestrictedKeywordRepository.GetAllAsync();

                var violatedKeyword = restrictedKeywords?.FirstOrDefault(k =>
                    !string.IsNullOrWhiteSpace(k.Keyword) &&
                    ideaNormalizedForMod.Contains(k.Keyword.Normalize(System.Text.NormalizationForm.FormC).Trim(), StringComparison.OrdinalIgnoreCase)
                );

                if (violatedKeyword != null)
                {
                    throw new ArgumentException("Ý tưởng của bạn nhỏ có từ ngữ chưa phù hợp, bé hãy mô tả lại thật đáng yêu nhé!");
                }
            }

            // 3. Lấy bản vẽ gốc (Drawing) và kiểm tra hồ sơ bé / phụ huynh
            var drawing = await _unitOfWork.DrawingRepository.GetByIdAsync(request.DrawingId);
            if (drawing == null)
                throw new KeyNotFoundException("Không tìm thấy bức tranh vẽ gốc.");

            var childProfile = await _unitOfWork.ChildProfileRepository.GetByIdAsync(drawing.ChildProfileId);
            if (childProfile == null)
                throw new KeyNotFoundException("Không tìm thấy hồ sơ của bé (ChildProfile).");

            if (!request.IsRegenerate && childProfile.RemainingCharacterLimit <= 0)
                throw new InvalidOperationException("Bé đã hết lượt tạo nhân vật trong ngày.");

            var parentProfile = await _unitOfWork.ParentProfileRepository.GetByIdAsync(childProfile.ParentProfileId);
            if (parentProfile == null)
                throw new KeyNotFoundException("Không tìm thấy hồ sơ phụ huynh (ParentProfile).");

            var featureKey = request.IsRegenerate ? "CharacterRegeneration" : "CharacterGeneration";
            var defaultCost = request.IsRegenerate ? 50 : 150;
            var creditCost = await _unitOfWork.CreditCostRepository.GetCostByFeatureKeyAsync(featureKey, defaultCost);

            if (parentProfile.RemainingMonthlyCredit < creditCost)
            {
                throw new InvalidOperationException(
                    $"Số dư Credit của phụ huynh không đủ để tạo nhân vật (cần {creditCost} credits, hiện còn {parentProfile.RemainingMonthlyCredit} credits)."
                );
            }

            // 4. XỬ LÝ CHAR_TYPE: AUTO-MATCH HOẶC FALLBACK "SINH VẬT GIẢ TƯỞNG"
            var activeCharTypes = await _unitOfWork.CharTypeRepository.GetActiveCharTypesAsync();
            Guid finalCharTypeId = request.CharTypeId ?? Guid.Empty;
            string charTypeName = string.Empty;

            if (hasAdditionalIdea)
            {
                var ideaNormalized = request.ChildAdditionalIdea!
                    .Normalize(System.Text.NormalizationForm.FormC)
                    .Trim()
                    .ToLowerInvariant();

                // Quét nhanh tìm CharType khớp với từ bé nhập (so sánh chuẩn Unicode Form C)
                // Sắp xếp giảm dần theo độ dài tên để ưu tiên cụm từ dài nhất (vd: "xương rồng" trước "rồng")
                var matched = activeCharTypes
                    .Where(c =>
                        (!string.IsNullOrWhiteSpace(c.NameVi) && ideaNormalized.Contains(c.NameVi.Normalize(System.Text.NormalizationForm.FormC).Trim().ToLowerInvariant())) ||
                        (!string.IsNullOrWhiteSpace(c.NameEn) && ideaNormalized.Contains(c.NameEn.Normalize(System.Text.NormalizationForm.FormC).Trim().ToLowerInvariant()))
                    )
                    .OrderByDescending(c => c.NameVi?.Length ?? 0)
                    .FirstOrDefault();

                if (matched != null)
                {
                    finalCharTypeId = matched.Id;
                    charTypeName = !string.IsNullOrWhiteSpace(matched.NameVi) ? matched.NameVi : matched.NameEn;
                }
                else if (finalCharTypeId == Guid.Empty)
                {
                    // Fallback về "Sinh vật giả tưởng" nếu không khớp loài nào trong danh mục
                    var fallback = activeCharTypes.FirstOrDefault(c =>
                        (!string.IsNullOrWhiteSpace(c.NameVi) && c.NameVi.ToLower().Contains("giả tưởng")) ||
                        (!string.IsNullOrWhiteSpace(c.NameEn) && c.NameEn.ToLower().Contains("fantasy creature"))
                    );

                    fallback ??= activeCharTypes.FirstOrDefault(c => c.Category == CharTypeCategory.Fantasy);
                    fallback ??= activeCharTypes.FirstOrDefault();

                    if (fallback != null)
                    {
                        finalCharTypeId = fallback.Id;
                    }

                    // Tên hiển thị trong prompt luôn là "sinh vật giả tưởng", không được lấy tên loài khác (như rồng) gán vào prompt
                    charTypeName = "sinh vật giả tưởng";
                }
            }

            // Nếu bé chọn từ 3 gợi ý hoặc đã có CharTypeId hợp lệ
            if (finalCharTypeId != Guid.Empty && string.IsNullOrWhiteSpace(charTypeName))
            {
                var selected = activeCharTypes.FirstOrDefault(c => c.Id == finalCharTypeId);
                if (selected != null)
                {
                    charTypeName = !string.IsNullOrWhiteSpace(selected.NameVi) ? selected.NameVi : selected.NameEn;
                }
                else
                {
                    charTypeName = await _unitOfWork.CharTypeRepository.GetCharTypeNameAsync(finalCharTypeId);
                }
            }

            if (finalCharTypeId == Guid.Empty)
            {
                throw new ArgumentException("Không xác định được loại nhân vật hợp lệ.");
            }

            // 5. TỔNG HỢP CÂU PROMPT SINH ẢNH HOÀN CHỈNH (finalPrompt)
            var visualTraits = (request.DetectedTags != null && request.DetectedTags.Any())
                ? string.Join(", ", request.DetectedTags)
                : "vibrant colors";

            // Ràng buộc nghiêm ngặt: Chỉ vẽ duy nhất 1 nhân vật chủ thể, toàn thân, ở giữa khung hình,
            // trên nền trắng phẳng tuyệt đối (pure solid white background), không có cảnh vật phụ để ClearBackdrop tách nền hoàn hảo nhất.
            const string isolationConstraint = 
                "Single full-body character centered in frame, isolated on a pure solid flat white background, zero shadows, no floor, no ground. No scenery, no background elements, no landscape, no extra objects, no other characters, no humans. Clean die-cut character sticker asset style, child-safe, 2D picture book illustration.";

            string finalPrompt;
            if (hasAdditionalIdea)
            {
                // Nhánh 2.2: Lấy ý tưởng bé nhập làm trung tâm, loại bỏ standardizedPrompt cũ của AI
                finalPrompt = $"A cute 2D children's storybook character asset illustration of a single {charTypeName} based on child's idea: '{request.ChildAdditionalIdea!.Trim()}'. Visual traits: {visualTraits}. {isolationConstraint}";
            }
            else
            {
                // Nhánh 2.1: Bé chọn gợi ý AI
                var basePrompt = !string.IsNullOrWhiteSpace(request.StandardizedPrompt)
                    ? request.StandardizedPrompt.Trim()
                    : $"A cute 2D children's storybook character asset illustration of a single {charTypeName}";

                finalPrompt = $"{basePrompt}. Confirmed character species: a single {charTypeName}. Visual traits: {visualTraits}. {isolationConstraint}";
            }

            var detectedTagsJson = request.DetectedTags != null && request.DetectedTags.Any()
                ? JsonSerializer.Serialize(request.DetectedTags)
                : null;

            // 6. Khởi tạo bản ghi AICharacterGenerate
            var aiCharacterGenerate = new AICharacterGenerate
            {
                ChildProfileId = drawing.ChildProfileId,
                DrawingId = drawing.Id,
                CharTypeId = finalCharTypeId,
                AIPromptUsed = finalPrompt,
                AIDetectedTagsJson = detectedTagsJson,
                AIGenStatus = AIGenStatus.Processing
            };

            await _unitOfWork.Repository<AICharacterGenerate>().AddAsync(aiCharacterGenerate);
            await _unitOfWork.CompleteAsync();

            // 7. Gọi Gemini/Flux sinh ảnh nhân vật & upload lên Cloudinary
            string generatedImageUrl;
            try
            {
                generatedImageUrl = await _aiService.GenerateImageAsync(finalPrompt, drawing.OriginalImageUrl);
            }
            catch (Exception ex)
            {
                aiCharacterGenerate.AIGenStatus = AIGenStatus.Failed;
                aiCharacterGenerate.ErrorMessage = ex.Message;
                _unitOfWork.Repository<AICharacterGenerate>().Update(aiCharacterGenerate);
                await _unitOfWork.CompleteAsync();

                // Rollback hoàn trả lại Credit và lượt tạo ngày (chưa bị trừ nên bảo toàn 100%)
                throw new InvalidOperationException("Hệ thống đang bận một chút, đã hoàn lại lượt tạo cho bé, bé hãy thử lại sau ít phút nhé!", ex);
            }

            // 8. Cập nhật AICharacterGenerate (Success) & Tạo Character ở trạng thái Preview
            aiCharacterGenerate.GenerateImageUrl = generatedImageUrl;
            aiCharacterGenerate.AIGenStatus = AIGenStatus.Success;
            _unitOfWork.Repository<AICharacterGenerate>().Update(aiCharacterGenerate);

            var characterName = hasAdditionalIdea
                ? (request.ChildAdditionalIdea!.Trim().Length > 50 ? request.ChildAdditionalIdea!.Trim().Substring(0, 50) : request.ChildAdditionalIdea!.Trim())
                : charTypeName;

            var character = new Character
            {
                ChildProfileId = drawing.ChildProfileId,
                AICharacterGenerateId = aiCharacterGenerate.Id,
                Name = characterName,
                DefaultPronoun = "Bạn nhỏ",
                FinalImageUrl = generatedImageUrl,
                Status = CharacterStatus.Preview,
                IsFavorite = false
            };
            await _unitOfWork.Repository<Character>().AddAsync(character);

            // 9. Trừ Credit của Phụ huynh, trừ lượt tạo trong ngày của Bé (nếu tạo lần đầu) & Ghi Log
            var creditBefore = parentProfile.RemainingMonthlyCredit;
            parentProfile.RemainingMonthlyCredit -= creditCost;
            _unitOfWork.ParentProfileRepository.Update(parentProfile);

            if (!request.IsRegenerate)
            {
                childProfile.RemainingCharacterLimit = Math.Max(0, childProfile.RemainingCharacterLimit - 1);
                _unitOfWork.ChildProfileRepository.Update(childProfile);
            }

            await _unitOfWork.CreditTransactionLogRepository.AddAsync(new CreditTransactionLog
            {
                ParentProfileId = parentProfile.Id,
                ActionType = ActionType.Spend,
                FeatureKey = featureKey,
                Amount = creditCost,
                CreditBefore = creditBefore,
                CreditAfter = parentProfile.RemainingMonthlyCredit,
                Description = request.IsRegenerate
                    ? $"Tạo lại ảnh nhân vật '{characterName}' (DrawingId: {drawing.Id})"
                    : $"Tạo ảnh nhân vật '{characterName}' lần đầu (DrawingId: {drawing.Id})"
            });

            await _unitOfWork.Repository<AIUsageLog>().AddAsync(new AIUsageLog
            {
                Provider = "GoogleGemini",
                ActionType = AIActionType.CharacterGenerate,
                TokenUsed = 0,
                CostAmount = creditCost
            });

            await _unitOfWork.CompleteAsync();

            // 10. Trả kết quả về Frontend
            return new GenerateCharacterResponse
            {
                AiCharacterGenerateId = aiCharacterGenerate.Id,
                CharacterId = character.Id,
                DrawingId = drawing.Id,
                CharTypeId = finalCharTypeId,
                GenerateImageUrl = generatedImageUrl,
                AiPromptUsed = finalPrompt,
                AiGenStatus = aiCharacterGenerate.AIGenStatus.ToString(),
                CharacterStatus = character.Status.ToString(),
                RemainingParentCredits = parentProfile.RemainingMonthlyCredit,
                RemainingDailyCharacterLimit = childProfile.RemainingCharacterLimit
            };
        }
    }
}