using Microsoft.EntityFrameworkCore;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Interfaces;
using sketch_tale.Infrastructure.Data;

namespace sketch_tale.Infrastructure.Repositories
{
    public class CreditCostRepository : GenericRepository<CreditCost>, ICreditCostRepository
    {
        public CreditCostRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<int> GetCostByFeatureKeyAsync(string featureKey, int defaultCost)
        {
            var creditCost = await _context.CreditCosts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.FeatureKey == featureKey && x.IsActive);

            return creditCost?.Cost ?? defaultCost;
        }
    }
}