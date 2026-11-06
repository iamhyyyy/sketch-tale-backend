using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.DTOs;

public class CharTypeDto : BaseFieldDto
{
    public string NameEn { get; set; } = null!;
    public string NameVi { get; set; } = null!;
    public CharTypeCategory Category { get; set; }
    public bool CanFly { get; set; }
    public bool CanSwim { get; set; }
    public CommonStatus Status { get; set; }
    public string? ImageUrl { get; set; }
}

public class CreateCharTypeDto
{
    public string NameEn { get; set; } = null!;
    public string NameVi { get; set; } = null!;
    public CharTypeCategory Category { get; set; }
    public bool CanFly { get; set; }
    public bool CanSwim { get; set; }
    public CommonStatus Status { get; set; }
    public string? ImageUrl { get; set; }
}

public class UpdateCharTypeDto
{
    public string NameEn { get; set; } = null!;
    public string NameVi { get; set; } = null!;
    public CharTypeCategory Category { get; set; }
    public bool CanFly { get; set; }
    public bool CanSwim { get; set; }
    public CommonStatus Status { get; set; }
    public string? ImageUrl { get; set; }
}
