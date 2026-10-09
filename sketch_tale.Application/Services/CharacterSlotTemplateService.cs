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

public class CharacterSlotTemplateService : ICharacterSlotTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;
    public CharacterSlotTemplateService(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cloudinaryService = cloudinaryService;
    }
    public Task<CharacterSlotTemplateDto> CreateSlotAsync(Guid pageId, CreateCharacterSlotTemplateDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteSlotAsync(Guid slotId)
    {
        throw new NotImplementedException();
    }
}
