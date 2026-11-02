using sketch_tale.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Services
{
    public interface IPayOsService
    {
        Task<string> CreatePaymentLinkAsync(Guid parentProfileId, CreatePaymentDto dto);
        Task<bool> ProcessWebhookAsync(string jsonBody);
        Task<bool> CancelPaymentAsync(long orderCode);
    }
}
