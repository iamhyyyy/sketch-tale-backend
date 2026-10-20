using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.DTOs;

public class RestrictedKeywordDto : BaseFieldDto
{
    public string Keyword { get; set; } = null!;
    public string? Description { get; set; }
    public Domain.Enums.MatchType MatchType { get; set; }
    public SeverityLevel SeverityLevel { get; set; }
    public ModerationAction Action { get; set; }
    public CommonStatus Status { get; set; }
}

public class CreateRestrictedKeywordDto
{
    public string Keyword { get; set; } = null!;
    public string? Description { get; set; }
    public Domain.Enums.MatchType MatchType { get; set; }
    public SeverityLevel SeverityLevel { get; set; }
    public ModerationAction Action { get; set; }
    public CommonStatus Status { get; set; }
}

public class UpdateRestrictedKeywordDto
{
    public string Keyword { get; set; } = null!;
    public string? Description { get; set; }
    public Domain.Enums.MatchType MatchType { get; set; }
    public SeverityLevel SeverityLevel { get; set; }
    public ModerationAction Action { get; set; }
    public CommonStatus Status { get; set; }
}
