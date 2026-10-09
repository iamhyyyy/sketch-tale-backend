using sketch_tale.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Services;

public interface ICharacterSlotTemplateService
{
    //Character Slot trên trang
    Task<CharacterSlotTemplateDto> CreateSlotAsync(Guid pageId, CreateCharacterSlotTemplateDto dto);
    Task<bool> DeleteSlotAsync(Guid slotId);
}
