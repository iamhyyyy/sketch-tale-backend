using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class EducationTheme : BaseEntity
{
    public string NameEn { get; set; } = null!;
    public string NameVi { get; set; } = null!;
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }
}
