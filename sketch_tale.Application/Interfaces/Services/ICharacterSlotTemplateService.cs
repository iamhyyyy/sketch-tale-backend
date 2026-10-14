using sketch_tale.Application.DTOs;

namespace sketch_tale.Application.Interfaces.Services;

public interface ICharacterSlotTemplateService
{
    //Character Slot trên trang (đặt nhân vật vào vị trí trên trang truyện)
    Task<IEnumerable<CharacterSlotTemplateDto>> GetSlotsByPageAsync(Guid pageId);
    Task<CharacterSlotTemplateDto> CreateSlotAsync(Guid pageId, CreateCharacterSlotTemplateDto dto);
    Task<bool> UpdateSlotAsync(Guid slotId, UpdateCharacterSlotTemplateDto dto);
    Task<bool> DeleteSlotAsync(Guid slotId);
}
