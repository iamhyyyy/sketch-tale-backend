
using Microsoft.EntityFrameworkCore;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using sketch_tale.Domain.Interfaces;
using sketch_tale.Infrastructure.Data;

namespace sketch_tale.Infrastructure.Repositories
{
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<AuditLog>> GetByUserId(Guid id)
        {
            return await _context.AuditLogs
                .Where(p => p.CreateBy == id)
                .ToListAsync();
        }
        public async Task<List<AuditLog>> GetByEntityName(string name)
        {
            return await _context.AuditLogs
                .Where(p => p.EntityName == name)
                .ToListAsync();
        }
        public async Task<List<AuditLog>> GetByPrimaryKeyOfEntity(Guid id)
        {
            return await _context.AuditLogs
                .Where(p => p.PrimaryKey == id)
                .ToListAsync();
        }
    }
}