using sketch_tale.Domain.Common;

namespace sketch_tale.Domain.Entities;

public class ContentManagerProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public List<string> UploadImageUrl { get; set; } = new List<string>();
}
