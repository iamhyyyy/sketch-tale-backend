using sketch_tale.Application.DTOs;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.Interfaces
{
    public interface ISubscriptionService
    {
        Task<IEnumerable<SubscriptionDto>> GetAllAsync();
        Task<List<SubscriptionDto>> GetByParentProfileId(Guid id);
        Task<List<SubscriptionDto>> GetBySubscriptionPlanId(Guid id);
        Task<SubscriptionDto?> GetByIdAsync(Guid id);
        Task<SubscriptionDto> AddAsync(CreateSubscriptionDto dto);
        Task<bool> Update(Guid id, UpdateSubscriptionDto dto);
        Task<bool> UpdateSubscriptionForSuccessPayment(Guid id, SubscriptionStatus status);
        Task<bool> Delete(Guid id);

    }
}