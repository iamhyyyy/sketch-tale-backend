using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;

namespace SmartCarWash.Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SubscriptionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SubscriptionDto>> GetAllAsync()
        {
            var items = await _unitOfWork.SubscriptionRepository.GetAllAsyncIncludePlan();

            return _mapper.Map<IEnumerable<SubscriptionDto>>(items);
        }

        public async Task<SubscriptionDto?> GetByIdAsync(Guid id)
        {
            var item = await _unitOfWork.SubscriptionRepository.GetByIdAsync(id);

            return item == null ? null : _mapper.Map<SubscriptionDto>(item);
        }
        public async Task<List<SubscriptionDto>> GetByParentProfileId(Guid id)
        {
            var items = await _unitOfWork.SubscriptionRepository.GetByParentProfileId(id);

            return _mapper.Map<List<SubscriptionDto>>(items);
        }

        public async Task<List<SubscriptionDto>> GetBySubscriptionPlanId(Guid id)
        {
            var items = await _unitOfWork.SubscriptionRepository.GetBySubscriptionPlanId(id);

            return _mapper.Map<List<SubscriptionDto>>(items);
        }

        public async Task<SubscriptionDto> AddAsync(CreateSubscriptionDto dto)
        {
            var item = _mapper.Map<Subscription>(dto);

            await GetParentBySubs(item);//check parent có tồn tại ko
            var plan = await GetPlanBySubs(item); //check plan có tồn tại ko
            SetSubscriptionDate(item, plan.DurationDays);

            await _unitOfWork.SubscriptionRepository.AddAsync(item);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<SubscriptionDto>(item);
        }

        public async Task<bool> Update(Guid id, UpdateSubscriptionDto dto)
        {
            var item = await _unitOfWork.SubscriptionRepository.GetByIdAsync(id);
            if (item == null) return false;
            await GetParentBySubs(item);
            await GetPlanBySubs(item);

            _mapper.Map(dto, item);

            _unitOfWork.SubscriptionRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> UpdateSubscriptionForSuccessPayment(Guid id, SubscriptionStatus status)
        {
            var item = await _unitOfWork.SubscriptionRepository.GetByIdAsync(id);
            if (item == null) return false;

            if(item.Status == SubscriptionStatus.PendingPayment && status == SubscriptionStatus.Active)
            {
                item.Status = status;
                if (!(GetDate(item.StartDate) == UtcDate()))
                {
                    var plan = await GetPlanBySubs(item);
                    SetSubscriptionDate(item, plan.DurationDays);
                    UpdateParentProfile(await GetParentBySubs(item), plan);
                }

            }
            _unitOfWork.SubscriptionRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var item = await _unitOfWork.SubscriptionRepository.GetByIdAsync(id);
            if (item == null) return false;

            item.Status = SubscriptionStatus.Cancelled;

            _unitOfWork.SubscriptionRepository.Update(item);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        private void UpdateParentProfile(ParentProfile parentProfile, SubscriptionPlan plan)
        {
            parentProfile.ChildProfileLimit = plan.ChildProfileLimit;
            parentProfile.MonthlyCreditLimit = plan.MonthlyCreditLimit;
            parentProfile.CanExportStory = plan.CanExportStory;
            parentProfile.AccessFullStories = plan.AccessFullStories;
            _unitOfWork.ParentProfileRepository.Update(parentProfile);
        }
        private async Task<SubscriptionPlan> GetPlanBySubs(Subscription item)
        {
            var plan = await _unitOfWork.SubscriptionPlanRepository.GetByIdAsync(item.SubscriptionPlanId);
            return plan == null ? throw new KeyNotFoundException("Không tìm thấy gói đăng ký!") : plan;
        }

        private async Task<ParentProfile> GetParentBySubs(Subscription item)
        {
            var parentProfile = await _unitOfWork.ParentProfileRepository.GetByIdAsync(item.ParentProfileId);
            return parentProfile == null ? throw new KeyNotFoundException("Không tìm thấy thông tin của phụ huynh") : parentProfile;
        }

        private void SetSubscriptionDate(Subscription item, int durationDay)
        {
            item.StartDate = DateTime.UtcNow.AddHours(7);
            item.EndDate = item.StartDate.AddDays(durationDay);
        }

        public async Task<bool> CheckAndSetExpiredStatus()
        {
            var today = UtcDate(); // Ngày hiện tại

            //lấy các gói trạng thái Active
            var activeSubs = await _unitOfWork.SubscriptionRepository.GetActiveSubscription();
            var expiredItems = activeSubs.Where(x => GetDate(x.EndDate) < today).ToList();

            if (!expiredItems.Any()) return true;

            foreach (var item in expiredItems)
            {
                item.Status = SubscriptionStatus.Expired;
                _unitOfWork.SubscriptionRepository.Update(item);
                var freePlan = await _unitOfWork.SubscriptionPlanRepository.GetPlanByCode("FreePlan");
                if (freePlan != null) UpdateParentProfile(await GetParentBySubs(item), freePlan);
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        private DateOnly UtcDate()
        {
            return DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        }
        private DateOnly GetDate(DateTime dt)
        {
            return DateOnly.FromDateTime(dt);
        }
    }
}