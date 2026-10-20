using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.DTOs;

public class AuditLogDto : BaseFieldDto
{
    public string AuditAction { get; set; } = null!;      
    public string EntityName { get; set; } = null!;   
    public Guid PrimaryKey { get; set; }  

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? ChangedColumns { get; set; }
}
