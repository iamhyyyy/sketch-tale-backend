using sketch_tale.Application.DTOs;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLogDto>> GetAllAsync();
        Task<AuditLogDto?> GetByIdAsync(Guid id);
        Task<List<AuditLogDto>> GetByUserId(Guid id);
        Task<List<AuditLogDto>> GetByEntityName(string name);
        Task<List<AuditLogDto>> GetByPrimaryKeyOfEntity(Guid id);
    }
}