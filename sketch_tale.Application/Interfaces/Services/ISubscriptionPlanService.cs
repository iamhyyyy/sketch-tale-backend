using sketch_tale.Application.DTOs;

namespace sketch_tale.Application.Interfaces
{
    public interface ISubscriptionPlanService
    {
        Task<IEnumerable<SubscriptionPlanDto>> GetAllAsync();
        Task<SubscriptionPlanDto?> GetByIdAsync(Guid id);
        Task<SubscriptionPlanDto> AddAsync(CreateSubscriptionPlanDto dto);
        Task<bool> Update(Guid id, UpdateSubscriptionPlanDto dto);
        Task<bool> Delete(Guid id);
    }
}