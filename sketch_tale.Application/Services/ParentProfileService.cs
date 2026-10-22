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
            if (dto.SubscriptionPlanId == Guid.Empty)
            {
                throw new ArgumentException("SubscriptionPlanId không được để trống hoặc là Guid rỗng.");
            }

            var profile = await _unitOfWork.ParentProfileRepository.GetByIdAsync(id);
            if (profile == null) return false;

            // Kiểm tra gói dịch vụ (Subscription Plan) có tồn tại trong hệ thống (SeedData / Database) hay không
            var plan = await _unitOfWork.SubscriptionPlanRepository.GetByIdAsync(dto.SubscriptionPlanId);
            if (plan == null)
            {
                throw new KeyNotFoundException("Không tìm thấy gói đăng ký (Subscription Plan) tương ứng.");
            }

            // Cập nhật SubscriptionPlanId cho hồ sơ phụ huynh
            profile.SubscriptionPlanId = dto.SubscriptionPlanId;

            _unitOfWork.ParentProfileRepository.Update(profile);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> MarkTrialPlanUsedAsync(Guid id)
        {
            var profile = await _unitOfWork.ParentProfileRepository.GetByIdAsync(id);
            if (profile == null) return false;

            // Đánh dấu đã xài bản trial
            profile.TrialPlusPlan = true;

            _unitOfWork.ParentProfileRepository.Update(profile);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
