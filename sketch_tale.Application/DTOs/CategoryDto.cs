using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.DTOs;

public class CategoryDto : BaseFieldDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }
}

public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }
}
