
using Microsoft.EntityFrameworkCore;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using sketch_tale.Domain.Interfaces;
using sketch_tale.Infrastructure.Data;

namespace sketch_tale.Infrastructure.Repositories
{
    public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
    {
        public SubscriptionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Subscription>> GetAllAsyncIncludePlan()
        {
            return await _context.Subscriptions
                .Include(s => s.Plan)
                .ToListAsync();
        }

        public new async Task<Subscription?> GetByIdAsync(Guid id)
        {
            return await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Subscription>> GetActiveSubscription()
        {
            return await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(p => p.Status == SubscriptionStatus.Active)
                .ToListAsync();
        }

        public async Task<List<Subscription>> GetByParentProfileId(Guid id)
        {
            return await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(p => p.ParentProfileId == id)
                .ToListAsync();
        }
        public async Task<List<Subscription>> GetBySubscriptionPlanId(Guid id)
        {
            return await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(p => p.SubscriptionPlanId == id)
                .ToListAsync();
        }
    }
}