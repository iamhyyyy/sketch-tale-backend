using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Services;

public class StoryQuizTemplateService : IStoryQuizTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;
    public StoryQuizTemplateService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public Task<StoryQuizTemplateDto> CreateQuizAsync(Guid storyId, CreateStoryQuizTemplateDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteQuizAsync(Guid quizId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<StoryQuizTemplateDto>> GetQuizzesByStoryAsync(Guid storyId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateQuizAsync(Guid quizId, CreateStoryQuizTemplateDto dto)
    {
        throw new NotImplementedException();
    }
}
