using sketch_tale.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Services
{
    public interface ICharacterApprovalService
    {
        Task<List<CharacterApprovalDto>> GetPendingCharactersForParentAsync(Guid parentProfileId);
        Task<CharacterApprovalDto> GetCharacterDetailForParentAsync(Guid characterId, Guid parentProfileId);
        Task<bool> ApproveCharacterAsync(Guid characterId, Guid parentProfileId);
        Task<bool> RejectCharacterAsync(Guid characterId, Guid parentProfileId);
        Task<bool> AcceptCharacterPreviewAsync(Guid characterId, Guid childProfileId);
        Task<bool> CancelCharacterPreviewAsync(Guid characterId, Guid childProfileId);
    }
}

