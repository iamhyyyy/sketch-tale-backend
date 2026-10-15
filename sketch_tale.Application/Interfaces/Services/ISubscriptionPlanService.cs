using sketch_tale.Application.DTOs;
using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Interfaces
{
    public interface ISubscriptionPlanService
    {
        Task<IEnumerable<SubscriptionPlanDto>> GetAllAsync();
        Task<SubscriptionPlanDto?> GetByIdAsync(Guid id);
        Task<SubscriptionPlanDto?> GetPlanByCode(string planCode);
        Task<SubscriptionPlanDto> AddAsync(CreateSubscriptionPlanDto dto);
        Task<bool> Update(Guid id, UpdateSubscriptionPlanDto dto);
        Task<bool> Delete(Guid id);
    }
}