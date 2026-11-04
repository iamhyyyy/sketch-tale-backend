using sketch_tale.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sketch_tale.Domain.Entities;

public class ContentManagerProfile : BaseEntity
{
    [Key]
    [ForeignKey(nameof(User))]
    public Guid ContentManagerProfileId { get; set; }
    public List<string> UploadImageUrl { get; set; } = new List<string>();

    public virtual User User { get; set; } = null!;
}
