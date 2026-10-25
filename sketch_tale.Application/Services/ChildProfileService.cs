using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Enums;
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
            var parentProfile = await _unitOfWork.ParentProfileRepository.GetByIdWithDetailsAsync(parentProfileId);
            return _mapper.Map<ParentProfileDto>(parentProfile);
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

        public async Task<bool> UpdateChildProfileAsync(Guid id, UpdateChildProfileDto dto)
        {
            var profile = await _unitOfWork.ChildProfileRepository.GetByIdAsync(id);
            if (profile == null) return false;

            if (!string.IsNullOrWhiteSpace(dto.NickName))
            {
                profile.NickName = dto.NickName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.TargetAgeGroup))
            {
                if (Enum.TryParse<TargetAgeGroup>(dto.TargetAgeGroup, true, out var ageGroup))
                {
                    profile.TargetAgeGroup = ageGroup;
                }
                else
                {
                    throw new ArgumentException($"Nhóm tuổi (TargetAgeGroup) không hợp lệ: '{dto.TargetAgeGroup}'. Các giá trị hợp lệ: {string.Join(", ", Enum.GetNames<TargetAgeGroup>())}");
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.ReadingLevel))
            {
                if (Enum.TryParse<ReadingLevel>(dto.ReadingLevel, true, out var readingLevel))
                {
                    profile.ReadingLevel = readingLevel;
                }
                else
                {
                    throw new ArgumentException($"Cấp độ đọc (ReadingLevel) không hợp lệ: '{dto.ReadingLevel}'. Các giá trị hợp lệ: {string.Join(", ", Enum.GetNames<ReadingLevel>())}");
                }
            }

            if (dto.DailyTimeLimit.HasValue)
            {
                if (dto.DailyTimeLimit.Value < 0)
                    throw new ArgumentException("Thời gian giới hạn mỗi ngày (DailyTimeLimit) không được âm.");
                profile.DailyTimeLimit = dto.DailyTimeLimit.Value;
            }

            if (dto.RemainingTimeLimit.HasValue)
            {
                if (dto.RemainingTimeLimit.Value < 0)
                    throw new ArgumentException("Thời gian còn lại (RemainingTimeLimit) không được âm.");
                profile.RemainingTimeLimit = dto.RemainingTimeLimit.Value;
            }

            if (dto.DailyCharacterLimit.HasValue)
            {
                if (dto.DailyCharacterLimit.Value < 0)
                    throw new ArgumentException("Giới hạn tạo nhân vật mỗi ngày (DailyCharacterLimit) không được âm.");
                profile.DailyCharacterLimit = dto.DailyCharacterLimit.Value;
            }

            if (dto.RemainingCharacterLimit.HasValue)
            {
                if (dto.RemainingCharacterLimit.Value < 0)
                    throw new ArgumentException("Lượt tạo nhân vật còn lại (RemainingCharacterLimit) không được âm.");
                profile.RemainingCharacterLimit = dto.RemainingCharacterLimit.Value;
            }

            profile.UpdatedAt = DateTime.UtcNow.AddHours(7);

            _unitOfWork.ChildProfileRepository.Update(profile);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
