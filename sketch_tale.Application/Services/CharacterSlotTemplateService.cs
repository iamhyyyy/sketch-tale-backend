using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Services;

public class CharacterSlotTemplateService : ICharacterSlotTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CharacterSlotTemplateService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CharacterSlotTemplateDto>> GetSlotsByPageAsync(Guid pageId)
    {
        var slots = await _unitOfWork.CharacterSlotTemplateRepository.GetSlotsByPageIdAsync(pageId);
        return _mapper.Map<IEnumerable<CharacterSlotTemplateDto>>(slots);
    }

    public async Task<CharacterSlotTemplateDto> CreateSlotAsync(Guid pageId, CreateCharacterSlotTemplateDto dto)
    {
        // 1. Kiểm tra trang tồn tại
        var page = await _unitOfWork.StoryPageTemplateRepository.GetByIdAsync(pageId);
        if (page == null)
        {
            throw new KeyNotFoundException($"Story page with id {pageId} was not found.");
        }

        // 2. Kiểm tra Role tồn tại
        var role = await _unitOfWork.StoryRoleTemplateRepository.GetByIdAsync(dto.StoryRoleTemplateId);
        if (role == null)
        {
            throw new KeyNotFoundException($"Story role with id {dto.StoryRoleTemplateId} was not found.");
        }

        // 3. Kiểm tra Role này thuộc cùng một Story với Page không
        if (role.StoryTemplateId != page.StoryTemplateId)
        {
            throw new InvalidOperationException("Role và Page không thuộc cùng một Story Template.");
        }

        // 4. Kiểm tra Role này đã có slot trên trang này chưa (mỗi Role chỉ xuất hiện 1 lần / trang)
        var existingSlots = await _unitOfWork.CharacterSlotTemplateRepository.GetSlotsByPageIdAsync(pageId);
        if (existingSlots.Any(s => s.StoryRoleTemplateId == dto.StoryRoleTemplateId))
        {
            throw new InvalidOperationException($"Nhân vật này đã được đặt trên trang {pageId} rồi.");
        }

        // 5. Tạo CharacterSlot mới
        var entity = _mapper.Map<CharacterSlotTemplate>(dto);
        entity.Id = Guid.NewGuid();
        entity.StoryPageId = pageId;
        entity.Status = Domain.Enums.CommonStatus.Active;

        await _unitOfWork.CharacterSlotTemplateRepository.AddAsync(entity);
        await _unitOfWork.CompleteAsync();

        return _mapper.Map<CharacterSlotTemplateDto>(entity);
    }

    public async Task<bool> UpdateSlotAsync(Guid slotId, UpdateCharacterSlotTemplateDto dto)
    {
        var slot = await _unitOfWork.CharacterSlotTemplateRepository.GetByIdAsync(slotId);
        if (slot == null) return false;

        _mapper.Map(dto, slot);

        _unitOfWork.CharacterSlotTemplateRepository.Update(slot);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<bool> DeleteSlotAsync(Guid slotId)
    {
        var slot = await _unitOfWork.CharacterSlotTemplateRepository.GetByIdAsync(slotId);
        if (slot == null) return false;

        _unitOfWork.CharacterSlotTemplateRepository.Delete(slot);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
