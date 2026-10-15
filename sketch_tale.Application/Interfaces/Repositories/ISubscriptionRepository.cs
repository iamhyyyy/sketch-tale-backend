

using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using System;

namespace sketch_tale.Domain.Interfaces;

public interface ISubscriptionRepository : IGenericRepository<Subscription>
{
    Task<List<Subscription>> GetAllAsyncIncludePlan();
    Task<List<Subscription>> GetActiveSubscription();
    Task<List<Subscription>> GetByParentProfileId(Guid id);
    Task<List<Subscription>> GetBySubscriptionPlanId(Guid id);
}