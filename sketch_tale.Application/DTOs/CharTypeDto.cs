using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.DTOs;

public class CharTypeDto : BaseFieldDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }
}

public class CreateCharTypeDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }
}

public class UpdateCharTypeDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }
}
