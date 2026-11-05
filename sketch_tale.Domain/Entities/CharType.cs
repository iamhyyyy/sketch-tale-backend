
using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class CharType : BaseEntity
{
    public string NameEn { get; set; } = null!;
    public string NameVi { get; set; } = null!;
    public CharTypeCategory Category { get; set; }
    public bool CanFly { get; set; } = false;
    public bool CanSwim { get; set; } = false;
    public CommonStatus Status { get; set; }
    public string? ImageUrl { get; set; }
}
