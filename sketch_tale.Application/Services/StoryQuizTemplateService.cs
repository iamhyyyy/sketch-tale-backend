using AutoMapper;
using Microsoft.AspNetCore.Http;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using System.Text.Json;

namespace sketch_tale.Application.Services;

public class StoryQuizTemplateService : IStoryQuizTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;

    public StoryQuizTemplateService(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cloudinaryService = cloudinaryService;
    }
    //lấy status active
    public async Task<IEnumerable<StoryQuizTemplateDto>> GetQuizzesByStoryAsync(Guid storyId)
    {
        var story = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(storyId);
        if (story == null || story.Status == CommonStatus.Inactive)
        {
            throw new KeyNotFoundException($"Story template với Id {storyId} không tồn tại.");
        }

        var quizzes = await _unitOfWork.StoryQuizTemplateRepository.GetQuizzesByStoryIdAsync(storyId);
        return _mapper.Map<IEnumerable<StoryQuizTemplateDto>>(quizzes);
    }
    //Lấy All tất cả status
    public async Task<IEnumerable<StoryQuizTemplateDto>> GetAllQuizzesByStoryAsync(Guid storyId)
    {
        var quizzes = await _unitOfWork.StoryQuizTemplateRepository.GetAllQuizzesByStoryIdAsync(storyId);
        return _mapper.Map<IEnumerable<StoryQuizTemplateDto>>(quizzes);
    }

    public async Task<StoryQuizTemplateDto?> GetQuizByIdAsync(Guid quizId)
    {
        var quiz = await _unitOfWork.StoryQuizTemplateRepository.GetByIdAsync(quizId);
        if (quiz == null || quiz.Status == CommonStatus.Inactive) return null;

        return _mapper.Map<StoryQuizTemplateDto>(quiz);
    }

    public async Task<StoryQuizTemplateDto> CreateQuizAsync(Guid storyId, CreateStoryQuizTemplateDto dto, IFormFile? file = null, IFormFile? audioFile = null)
    {
        var story = await _unitOfWork.StoryTemplateRepository.GetByIdAsync(storyId);
        if (story == null || story.Status == CommonStatus.Inactive)
        {
            throw new KeyNotFoundException($"Story template với Id {storyId} không tồn tại.");
        }

        if (string.IsNullOrWhiteSpace(dto.QuestionText))
        {
            throw new ArgumentException("Nội dung câu hỏi không được để trống.");
        }

        if (dto.Options == null || dto.Options.Count < 2)
        {
            throw new ArgumentException("Câu hỏi trắc nghiệm cần có ít nhất 2 đáp án lựa chọn.");
        }

        if (dto.CorrectOptionIndex < 0 || dto.CorrectOptionIndex >= dto.Options.Count)
        {
            throw new ArgumentException($"Đáp án đúng (CorrectOptionIndex = {dto.CorrectOptionIndex}) không hợp lệ so với số lượng lựa chọn ({dto.Options.Count}).");
        }

        var entity = _mapper.Map<StoryQuizTemplate>(dto);
        entity.Id = Guid.NewGuid();
        entity.StoryTemplateId = storyId;
        entity.Status = CommonStatus.Active;
        entity.OptionsJson = JsonSerializer.Serialize(dto.Options);

        if (file != null)
        {
            entity.QuestionPicUrl = await _cloudinaryService.UploadImageAsync(file, "SketchTale/quizzes");
        }
        if (audioFile != null)
        {
            entity.QuestionAudioUrl = await _cloudinaryService.UploadAudioAsync(audioFile, "SketchTale/quizz-audios");
        }
        await _unitOfWork.StoryQuizTemplateRepository.AddAsync(entity);
        await _unitOfWork.CompleteAsync();

        var resultDto = _mapper.Map<StoryQuizTemplateDto>(entity);
        resultDto.Options = dto.Options;
        return resultDto;
    }

    public async Task<bool> UpdateQuizAsync(Guid quizId, UpdateStoryQuizTemplateDto dto, IFormFile? file = null, IFormFile? audioFile = null)
    {
        var quiz = await _unitOfWork.StoryQuizTemplateRepository.GetByIdAsync(quizId);
        if (quiz == null || quiz.Status == CommonStatus.Inactive) return false;

        if (dto.Options != null && dto.Options.Any())
        {
            if (dto.Options.Count < 2)
            {
                throw new ArgumentException("Câu hỏi trắc nghiệm cần có ít nhất 2 đáp án lựa chọn.");
            }

            if (dto.CorrectOptionIndex < 0 || dto.CorrectOptionIndex >= dto.Options.Count)
            {
                throw new ArgumentException($"Đáp án đúng (CorrectOptionIndex = {dto.CorrectOptionIndex}) không hợp lệ so với số lượng lựa chọn ({dto.Options.Count}).");
            }

            quiz.OptionsJson = JsonSerializer.Serialize(dto.Options);
            quiz.CorrectOptionIndex = dto.CorrectOptionIndex;
        }

        if (!string.IsNullOrWhiteSpace(dto.QuestionText))
        {
            quiz.QuestionText = dto.QuestionText;
        }

        if (audioFile != null)
        {
            quiz.QuestionAudioUrl = await _cloudinaryService.UploadAudioAsync(audioFile, "SketchTale/quizz-audios");
        }

        if (file != null)
        {
            quiz.QuestionPicUrl = await _cloudinaryService.UploadImageAsync(file, "SketchTale/quizzes");
        }

        quiz.Status = dto.Status;
        quiz.UpdatedAt = DateTime.UtcNow.AddHours(7);

        _unitOfWork.StoryQuizTemplateRepository.Update(quiz);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<bool> DeleteQuizAsync(Guid quizId)
    {
        var quiz = await _unitOfWork.StoryQuizTemplateRepository.GetByIdAsync(quizId);
        if (quiz == null || quiz.Status == CommonStatus.Inactive) return false;

        quiz.Status = CommonStatus.Inactive;
        quiz.UpdatedAt = DateTime.UtcNow.AddHours(7);

        _unitOfWork.StoryQuizTemplateRepository.Update(quiz);
        await _unitOfWork.CompleteAsync();
        return true;
    }

   
}
