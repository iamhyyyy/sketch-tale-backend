using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class ContentReport : BaseEntity
{
    public string TargetType { get; set; } = string.Empty; // 'Character', 'Story', 'Comment'
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public CommonStatus Status { get; set; }
}
