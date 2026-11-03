using Audit.Core;
using Audit.EntityFramework;
using Audit.EntityFramework.Providers;
using Microsoft.Extensions.DependencyInjection;
using sketch_tale.Domain.Entities;
using System.Text.Json;

namespace sketch_tale.Infrastructure.Auditing;

public static class AuditConfiguration
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection services)
    {
        global::Audit.Core.Configuration.DataProvider = new EntityFrameworkDataProvider
        {
            AuditTypeMapper = (entityType, entry) =>
                entityType == typeof(AuditLog) ? null : typeof(AuditLog),

            AuditEntityAction = HandleAuditEntityAction
        };

        return services;
    }

    // Khai báo chính xác Task<bool>
    private static Task<bool> HandleAuditEntityAction(AuditEvent evt, EventEntry entry, object auditEntity)
    {
        if (auditEntity is not AuditLog auditLog)
        {
            return Task.FromResult(true);
        }

        auditLog.Id = Guid.NewGuid();
        auditLog.CreatedAt = DateTime.UtcNow.AddHours(7);
        auditLog.CreateBy = Guid.Empty;
        auditLog.UpdatedAt = null;
        auditLog.UpdateBy = null;

        auditLog.AuditAction = entry.Action;
        auditLog.EntityName = entry.EntityType.Name;

        var pk = entry.PrimaryKey.Values.FirstOrDefault();
        auditLog.PrimaryKey = pk != null && Guid.TryParse(pk.ToString(), out var parsedPk)
            ? parsedPk
            : Guid.Empty;

        if (entry.Changes != null && entry.Changes.Count > 0)
        {
            ProcessEntityChanges(entry, auditLog);
        }
        else
        {
            auditLog.OldValues = entry.Action == "Delete" ? entry.ToJson() : null;
            auditLog.NewValues = entry.Action == "Insert" ? entry.ToJson() : null;
            auditLog.ChangedColumns = null;
        }

        return Task.FromResult(true); // Trả về Task<bool> với giá trị true
    }

    private static void ProcessEntityChanges(EventEntry entry, AuditLog auditLog)
    {
        var ignoredColumns = new HashSet<string> { "UpdatedAt", "UpdatedBy" };

        var realChanges = entry.Changes
            .Where(c => !ignoredColumns.Contains(c.ColumnName))
            .Where(c => !AreValuesEqual(c.OriginalValue, c.NewValue))
            .ToList();

        if (realChanges.Count > 0)
        {
            auditLog.OldValues = JsonSerializer.Serialize(realChanges.ToDictionary(c => c.ColumnName, c => c.OriginalValue));
            auditLog.NewValues = JsonSerializer.Serialize(realChanges.ToDictionary(c => c.ColumnName, c => c.NewValue));
            auditLog.ChangedColumns = string.Join(", ", realChanges.Select(c => c.ColumnName));
        }
        else
        {
            auditLog.OldValues = null;
            auditLog.NewValues = null;
            auditLog.ChangedColumns = null;
        }
    }

    private static bool AreValuesEqual(object? val1, object? val2)
    {
        if (val1 == null && val2 == null) return true;
        if (val1 == null || val2 == null) return false;
        return Equals(val1, val2) || string.Equals(val1.ToString(), val2.ToString());
    }
}