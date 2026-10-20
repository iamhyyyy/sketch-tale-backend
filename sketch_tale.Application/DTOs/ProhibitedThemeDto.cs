using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.DTOs;

public class ProhibitedThemeDto : BaseFieldDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Domain.Enums.MatchType MatchType { get; set; }
    public SeverityLevel SeverityLevel { get; set; }
    public ModerationAction Action { get; set; }
    public CommonStatus Status { get; set; }
}

public class CreateProhibitedThemeDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Domain.Enums.MatchType MatchType { get; set; }
    public SeverityLevel SeverityLevel { get; set; }
    public ModerationAction Action { get; set; }
    public CommonStatus Status { get; set; }
}

public class UpdateProhibitedThemeDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Domain.Enums.MatchType MatchType { get; set; }
    public SeverityLevel SeverityLevel { get; set; }
    public ModerationAction Action { get; set; }
    public CommonStatus Status { get; set; }
}
