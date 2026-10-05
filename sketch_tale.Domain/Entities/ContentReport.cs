using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class ContentReport : BaseEntity
{
    public ReportTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ContentReportStatus Status { get; set; }
}
