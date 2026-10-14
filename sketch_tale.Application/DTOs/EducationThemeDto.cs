using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.DTOs;

public class EducationThemeDto : BaseFieldDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }
}

public class CreateEducationThemeDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }
}

public class UpdateEducationThemeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }
}
