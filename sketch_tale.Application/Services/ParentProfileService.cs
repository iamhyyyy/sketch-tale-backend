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
    public class ParentProfileService : IParentProfileService
    {
       
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ParentProfileService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ParentProfileDto>> GetAllAsync()
        {
            var profiles = await _unitOfWork.ParentProfileRepository.GetAllWithDetailsAsync();
            return _mapper.Map<IEnumerable<ParentProfileDto>>(profiles);
        }

        public async Task<ParentProfileDto?> GetByIdAsync(Guid id)
        {
            var profile = await _unitOfWork.ParentProfileRepository.GetByIdWithDetailsAsync(id);
            return _mapper.Map<ParentProfileDto>(profile);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateParentProfileDto dto)
        {
            var profile = await _unitOfWork.ParentProfileRepository.GetByIdAsync(id);
            if (profile == null) return false;

            // Map đè các trường từ dto sang profile (ở đây là SecurityCode)
            _mapper.Map(dto, profile);

            _unitOfWork.ParentProfileRepository.Update(profile);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> UpdateProfileForSubscriptionAsync(Guid id, int childProfileLimit, int monthlyCreditLimit, bool canExportStory, bool accessFullStories)
        {
            var profile = await _unitOfWork.ParentProfileRepository.GetByIdAsync(id);
            if (profile == null) return false;

            // Cập nhật thông số giới hạn của gói cước
            profile.ChildProfileLimit = childProfileLimit;
            profile.MonthlyCreditLimit = monthlyCreditLimit;
            profile.CanExportStory = canExportStory;
            profile.AccessFullStories = accessFullStories;

            // Lưu ý: Tuỳ thuộc vào logic kinh doanh, bạn có thể cần cộng thêm 
            // vào RemainingChildProfileLimit và RemainingMonthlyCreditLimit ở đây.
            // Ví dụ: profile.RemainingMonthlyCreditLimit += monthlyCreditLimit;

            _unitOfWork.ParentProfileRepository.Update(profile);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> MarkTrialPlanUsedAsync(Guid id)
        {
            var profile = await _unitOfWork.ParentProfileRepository.GetByIdAsync(id);
            if (profile == null) return false;

            // Đánh dấu đã xài bản trial
            profile.SubscriptionTrialPlusPlan = true;

            _unitOfWork.ParentProfileRepository.Update(profile);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
