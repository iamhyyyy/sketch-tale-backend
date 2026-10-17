

using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using System;

namespace sketch_tale.Domain.Interfaces;

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    Task<List<AuditLog>> GetByUserId(Guid id);
    Task<List<AuditLog>> GetByEntityName(string name);
    Task<List<AuditLog>> GetByPrimaryKeyOfEntity(Guid id);
}