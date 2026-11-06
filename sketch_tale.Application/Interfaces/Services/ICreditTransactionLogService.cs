using sketch_tale.Application.DTOs;
using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Interfaces
{
    public interface ICreditTransactionLogService
    {
        Task<IEnumerable<CreditTransactionLogDto>> GetAllAsync();
        Task<CreditTransactionLogDto?> GetByIdAsync(Guid id);
        Task<List<CreditTransactionLogDto>> GetByUserIdAsync(Guid userId);
        Task<List<CreditTransactionLogDto>> GetByParentIdAsync(Guid parentId);
        Task<CreditTransactionLogDto> AddAsync(CreateCreditTransactionLogDto dto);
        Task<bool> Update(Guid id, UpdateCreditTransactionLogDto dto);
        Task<bool> Delete(Guid id);
    }
}