

using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;

namespace sketch_tale.Domain.Interfaces;

public interface ISubscriptionPlanRepository : IGenericRepository<SubscriptionPlan>
{
    Task<SubscriptionPlan?> GetPlanByCode(string planCode);
}