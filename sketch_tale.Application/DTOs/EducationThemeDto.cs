using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.DTOs;

public class EducationThemeDto : BaseFieldDto
{
    public string NameEn { get; set; } = null!;
    public string NameVi { get; set; } = null!;
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }
}

public class CreateEducationThemeDto
{
    public string NameEn { get; set; } = null!;
    public string NameVi { get; set; } = null!;
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }
}

public class UpdateEducationThemeDto
{
    public string NameEn { get; set; } = null!;
    public string NameVi { get; set; } = null!;
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }
}
