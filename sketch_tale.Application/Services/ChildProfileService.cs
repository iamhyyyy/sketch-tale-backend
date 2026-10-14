using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Services
{
    public class ChildProfileService : IChildProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ChildProfileService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<IEnumerable<ChildProfileDto>> GetAllChildProfilesAsync()
        {
                 var profiles = await _unitOfWork.ChildProfileRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<ChildProfileDto>>(profiles);
        }

        public async Task<ChildProfileDto?> GetByIdAsync(Guid id)
        {
            var profile = await _unitOfWork.ChildProfileRepository.GetByIdAsync(id);
            return _mapper.Map<ChildProfileDto>(profile);
        }

        public async Task<ParentProfileDto?> GetByParentProfileIdAsync(Guid parentProfileId)
        {
            //var parentProfile = await _unitOfWork.ParentProfileRepository.GetByIdAsync(parentProfileId);
            //return _mapper.Map<ParentProfileDto>(parentProfile);
            throw new NotImplementedException();
        }

        public async Task<int?> GetDailyCharacterLimitByIdAsync(Guid id)
        {
            var profile = await _unitOfWork.ChildProfileRepository.GetByIdAsync(id);
            return profile?.DailyCharacterLimit;
        }

        public async Task<int?> GetDailyTimeLimitByIdAsync(Guid id)
        {
            var profile = await _unitOfWork.ChildProfileRepository.GetByIdAsync(id);
            return profile?.DailyTimeLimit;
        }       

        public async Task<string?> GetNickNameByIdAsync(Guid id)
        {
            var profile = await _unitOfWork.ChildProfileRepository.GetByIdAsync(id);
            return profile?.NickName;
        
        }

        public async Task<string?> GetTargetAgeGroupByIdAsync(Guid id)
        {
            var profile = await _unitOfWork.ChildProfileRepository.GetByIdAsync(id);
            return profile?.TargetAgeGroup.ToString();
        }           

        public async Task<bool> UpdateAsync(Guid id, UpdateChildProfileDto dto)
        {
            var profile = await _unitOfWork.ChildProfileRepository.GetByIdAsync(id);
            if (profile == null) return false;

            _mapper.Map(dto, profile);
            _unitOfWork.ChildProfileRepository.Update(profile);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
