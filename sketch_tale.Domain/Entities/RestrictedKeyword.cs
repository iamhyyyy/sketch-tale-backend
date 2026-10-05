using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;
public class RestrictedKeyword : BaseEntity
{
    public string Keyword { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Enums.MatchType MatchType { get; set; } = Enums.MatchType.Contains;
    public SeverityLevel SeverityLevel { get; set; } = SeverityLevel.Medium;
    public ModerationAction Action { get; set; } = ModerationAction.Reject;
    public ModerationStatus Status { get; set; } = ModerationStatus.Pending;
}