using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.DTOs;

public class ContentReportDto : BaseFieldDto
{
    public ReportTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ContentReportStatus Status { get; set; }
}

public class CreateContentReportDto
{
    public ReportTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ContentReportStatus Status { get; set; }
}

public class UpdateContentReportDto
{
    public ReportTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ContentReportStatus Status { get; set; }
}
