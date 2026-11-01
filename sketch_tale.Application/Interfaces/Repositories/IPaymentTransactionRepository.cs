using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Repositories
{
    public interface IPaymentTransactionRepository : IGenericRepository<PaymentTransaction>
    {
        Task<PaymentTransaction?> GetByOrderCodeAsync(string orderCode);
        Task<PaymentTransaction?> GetByIdWithDetailsAsync(Guid id);
        Task<List<PaymentTransaction>> GetByParentProfileIdAsync(Guid parentProfileId);
        Task<List<PaymentTransaction>> GetAllWithDetailsAsync();
    }
}
