using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sketch_tale.Application.Services
{
    public class GeneratedStoryService : IGeneratedStoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GeneratedStoryService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        // 1. BÉ TẠO TRUYỆN
        public async Task<GeneratedStoryDto> GenerateStoryAsync(GenerateStoryRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Xác định ChildProfileId (từ request hoặc từ JWT token của bé)
            var childProfileId = request.ChildProfileId ?? Guid.Empty;
            if (childProfileId == Guid.Empty && Guid.TryParse(_currentUserService.UserId, out var currentUserId))
            {
                childProfileId = currentUserId;
            }

            if (childProfileId == Guid.Empty)
                throw new ArgumentException("ChildProfileId không hợp lệ hoặc không xác định được tài khoản bé.");

            // Kiểm tra hồ sơ bé
            var child = await _unitOfWork.ChildProfileRepository.GetByIdAsync(childProfileId);
            if (child == null)
                throw new KeyNotFoundException("Không tìm thấy hồ sơ của bé.");

            // Kiểm tra StoryTemplate tồn tại và đang hoạt động
            var storyTemplate = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(request.StoryTemplateId);
            if (storyTemplate == null || storyTemplate.Status == CommonStatus.Inactive)
                throw new KeyNotFoundException("Không tìm thấy câu chuyện mẫu hoặc câu chuyện chưa được kích hoạt.");

            // Tạo bản ghi GeneratedStory
            var generatedStory = new GeneratedStory
            {
                ChildProfileId = childProfileId,
                StoryTemplateId = request.StoryTemplateId,
                IsFavorite = false,
                Status = GeneratedStoryStatus.InProgress,
                LastPageRead = 0,
                CreatedAt = DateTime.UtcNow.AddHours(7),
                CreateBy = childProfileId
            };
            await _unitOfWork.GeneratedStoryRepository.AddAsync(generatedStory);
            await _unitOfWork.CompleteAsync();

            // Trả về DTO hoàn chỉnh
            var result = _mapper.Map<GeneratedStoryDto>(generatedStory);
            return result;
        }

        public async Task<GeneratedStoryDetailsDto> GetGeneratedStoryByIdAsync(Guid id)
        {
            var story = await _unitOfWork.GeneratedStoryRepository.GetByIdWithDetailsAsync(id);
            if (story == null || story.Status == GeneratedStoryStatus.Hidden)
                throw new KeyNotFoundException($"Không tìm thấy câu chuyện đã tạo với ID: {id}");

            return _mapper.Map<GeneratedStoryDetailsDto>(story);
        }

        public async Task<List<GeneratedStoryDetailsDto>> GetGeneratedStoriesByChildProfileIdAsync(Guid childProfileId)
        {
            var child = await _unitOfWork.ChildProfileRepository.GetByIdAsync(childProfileId);
            if (child == null)
                throw new KeyNotFoundException($"Không tìm thấy hồ sơ của bé với ID: {childProfileId}");

            var stories = await _unitOfWork.GeneratedStoryRepository.GetByChildProfileIdAsync(childProfileId);
            return _mapper.Map<List<GeneratedStoryDetailsDto>>(stories);
        }
    }
}
