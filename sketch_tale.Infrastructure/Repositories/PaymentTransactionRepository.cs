using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sketch_tale.Infrastructure.Repositories
{
    public class PaymentTransactionRepository : GenericRepository<PaymentTransaction>, IPaymentTransactionRepository
    {
        public PaymentTransactionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<PaymentTransaction?> GetByOrderCodeAsync(string orderCode)
        {
            return await _dbSet
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.Plan)
                .Include(p => p.ParentProfile)
                .FirstOrDefaultAsync(p => p.TxnRef == orderCode);
        }

        public async Task<PaymentTransaction?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.Plan)
                .Include(p => p.ParentProfile)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<PaymentTransaction>> GetByParentProfileIdAsync(Guid parentProfileId)
        {
            return await _dbSet
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.Plan)
                .Where(p => p.ParentProfileId == parentProfileId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PaymentTransaction>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.Plan)
                .Include(p => p.ParentProfile)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
