using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using System;

namespace sketch_tale.Application.Services
{
    public class UserStoryCharacterMappingService : IUserStoryCharacterMappingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserStoryCharacterMappingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserStoryCharacterMappingDto>> GetAllAsync()
        {
            var items = await _unitOfWork.UserStoryCharacterMappingRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<UserStoryCharacterMappingDto>>(items);
        }

        public async Task<UserStoryCharacterMappingDto?> GetByIdAsync(Guid id)
        {
            var item = await _unitOfWork.UserStoryCharacterMappingRepository.GetByIdAsync(id);

            return item == null ? null : _mapper.Map<UserStoryCharacterMappingDto>(item);
        }

        public async Task<List<UserStoryCharacterMappingDto>> GetByCharacterIdAsync(Guid charId)
        {
            var items = await _unitOfWork.UserStoryCharacterMappingRepository.GetByCharacterIdAsync(charId);

            return _mapper.Map<List<UserStoryCharacterMappingDto>>(items);
        }

        public async Task<List<UserStoryCharacterMappingDto>> GetByGenerateStoryIdAsync(Guid genStoryId)
        {
            var items = await _unitOfWork.UserStoryCharacterMappingRepository.GetByGenerateStoryIdAsync(genStoryId);

            return _mapper.Map<List<UserStoryCharacterMappingDto>>(items);
        }

        public async Task<UserStoryCharacterMappingDto> AddAsync(CreateUserStoryCharacterMappingDto dto)
        {
            var item = _mapper.Map<UserStoryCharacterMapping>(dto);

            var roleTemplate = await _unitOfWork.StoryRoleTemplateRepository.GetByIdAsync(item.StoryRoleTemplateId);

            if (roleTemplate == null) throw new Exception("Nhân vật trên không tồn tại");
            if (roleTemplate.RequiresParentApproval) item.ParentApprovalStatus = ParentApprovalStatus.Pending;
            else item.ParentApprovalStatus = ParentApprovalStatus.NotRequired;

            await _unitOfWork.UserStoryCharacterMappingRepository.AddAsync(item);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<UserStoryCharacterMappingDto>(item);
        }

        public async Task<bool> Update(Guid id, UpdateUserStoryCharacterMappingDto dto)
        {
            var item = await _unitOfWork.UserStoryCharacterMappingRepository.GetByIdAsync(id);
            if (item == null) return false;

            _mapper.Map(dto, item);

            _unitOfWork.UserStoryCharacterMappingRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var item = await _unitOfWork.UserStoryCharacterMappingRepository.GetByIdAsync(id);
            if (item == null) return false;

            item.ParentApprovalStatus = ParentApprovalStatus.Rejected;

            _unitOfWork.UserStoryCharacterMappingRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}