using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sketch_tale.Application.Services
{
    public class CharacterApprovalService : ICharacterApprovalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public CharacterApprovalService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<List<CharacterApprovalDto>> GetPendingCharactersForParentAsync(Guid parentProfileId)
        {
            var characters = await _unitOfWork.CharacterRepository.GetPendingCharactersByParentIdAsync(parentProfileId);
            return _mapper.Map<List<CharacterApprovalDto>>(characters);
        }

        public async Task<CharacterApprovalDto> GetCharacterDetailForParentAsync(Guid characterId, Guid parentProfileId)
        {
            var character = await _unitOfWork.CharacterRepository.GetCharacterWithDetailsAsync(characterId);
            if (character == null)
                throw new KeyNotFoundException("Không tìm thấy nhân vật.");

            if (character.ChildProfile == null || character.ChildProfile.ParentProfileId != parentProfileId)
                throw new UnauthorizedAccessException("Bạn không có quyền xem nhân vật này.");

            return _mapper.Map<CharacterApprovalDto>(character);
        }

        public async Task<bool> ApproveCharacterAsync(Guid characterId, Guid parentProfileId)
        {
            var character = await _unitOfWork.CharacterRepository.GetByIdAsync(characterId);
            if (character == null)
                throw new KeyNotFoundException("Không tìm thấy nhân vật.");

            var child = await _unitOfWork.ChildProfileRepository.GetByIdAsync(character.ChildProfileId);
            if (child == null || child.ParentProfileId != parentProfileId)
                throw new UnauthorizedAccessException("Bạn không có quyền thao tác trên nhân vật này.");

            if (character.Status != CharacterStatus.Pending)
                throw new InvalidOperationException($"Nhân vật đang ở trạng thái '{character.Status}', chỉ có thể phê duyệt khi ở trạng thái Chờ duyệt (Pending).");

            character.Status = CharacterStatus.Approved;
            _unitOfWork.CharacterRepository.Update(character);

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> RejectCharacterAsync(Guid characterId, Guid parentProfileId)
        {
            var character = await _unitOfWork.CharacterRepository.GetByIdAsync(characterId);
            if (character == null)
                throw new KeyNotFoundException("Không tìm thấy nhân vật.");

            var child = await _unitOfWork.ChildProfileRepository.GetByIdAsync(character.ChildProfileId);
            if (child == null || child.ParentProfileId != parentProfileId)
                throw new UnauthorizedAccessException("Bạn không có quyền thao tác trên nhân vật này.");

            if (character.Status != CharacterStatus.Pending)
                throw new InvalidOperationException($"Nhân vật đang ở trạng thái '{character.Status}', chỉ có thể từ chối khi ở trạng thái Chờ duyệt (Pending).");

            character.Status = CharacterStatus.Rejected;
            _unitOfWork.CharacterRepository.Update(character);

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> AcceptCharacterPreviewAsync(Guid characterId, Guid childProfileId)
        {
            if (childProfileId == Guid.Empty && Guid.TryParse(_currentUserService.UserId, out var currentUserId))
            {
                childProfileId = currentUserId;
            }

            var character = await _unitOfWork.CharacterRepository.GetByIdAsync(characterId);
            if (character == null)
                throw new KeyNotFoundException("Không tìm thấy nhân vật.");

            if (character.ChildProfileId != childProfileId)
                throw new UnauthorizedAccessException("Nhân vật không thuộc về bé này.");

            if (character.Status != CharacterStatus.Preview)
                throw new InvalidOperationException($"Nhân vật đang ở trạng thái '{character.Status}', chỉ có thể chấp nhận khi ở trạng thái Preview.");

            character.Status = CharacterStatus.Pending;
            _unitOfWork.CharacterRepository.Update(character);

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> CancelCharacterPreviewAsync(Guid characterId, Guid childProfileId)
        {
            if (childProfileId == Guid.Empty && Guid.TryParse(_currentUserService.UserId, out var currentUserId))
            {
                childProfileId = currentUserId;
            }

            var character = await _unitOfWork.CharacterRepository.GetByIdAsync(characterId);
            if (character == null)
                throw new KeyNotFoundException("Không tìm thấy nhân vật.");

            if (character.ChildProfileId != childProfileId)
                throw new UnauthorizedAccessException("Nhân vật không thuộc về bé này.");

            if (character.Status != CharacterStatus.Preview)
                throw new InvalidOperationException($"Nhân vật đang ở trạng thái '{character.Status}', chỉ có thể hủy khi ở trạng thái Preview.");

            character.Status = CharacterStatus.Hidden;
            _unitOfWork.CharacterRepository.Update(character);

            // Hoàn lại 1 lượt tạo trong ngày cho bé
            var child = await _unitOfWork.ChildProfileRepository.GetByIdAsync(childProfileId);
            if (child != null)
            {
                child.RemainingCharacterLimit += 1;
                _unitOfWork.ChildProfileRepository.Update(child);
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
