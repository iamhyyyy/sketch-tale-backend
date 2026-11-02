using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Services
{
    public interface IPaymentTransactionService
    {
       Task<PaymentTransactionDto> GetByOrderCodeAsync(string orderCode);
       Task<PaymentTransactionDto> GetByIdWithDetailsAsync(Guid id);
       Task<List<PaymentTransactionDto>> GetAllWithDetailsAsync();
       Task<bool> CancelPayment(long orderCode, long code);
    }
}
