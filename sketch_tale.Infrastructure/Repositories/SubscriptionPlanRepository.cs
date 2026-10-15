
using Microsoft.EntityFrameworkCore;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using sketch_tale.Domain.Interfaces;
using sketch_tale.Infrastructure.Data;

namespace sketch_tale.Infrastructure.Repositories
{
    public class SubscriptionPlanRepository : GenericRepository<SubscriptionPlan>, ISubscriptionPlanRepository
    {
        public SubscriptionPlanRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<SubscriptionPlan?> GetPlanByCode(string planCode)
        {
            return await _context.SubscriptionPlans
                .FirstOrDefaultAsync(x => x.Code == planCode);
        }
    }
}