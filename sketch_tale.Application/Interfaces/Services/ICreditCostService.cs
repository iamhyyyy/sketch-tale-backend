using sketch_tale.Application.DTOs;

namespace sketch_tale.Application.Interfaces
{
    public interface ICreditCostService
    {
        Task<IEnumerable<CreditCostDto>> GetAllAsync();
        Task<CreditCostDto?> GetByIdAsync(Guid id);
        Task<CreditCostDto> AddAsync(CreateCreditCostDto dto);
        Task<bool> Update(Guid id, UpdateCreditCostDto dto);
        Task<bool> Delete(Guid id);
    }
}