using sketch_tale.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sketch_tale.Domain.Entities;

public class ParentProfileSub : BaseEntityWithoutId
{
    public Guid UserId { get; set; }

    [Key]
    [ForeignKey(nameof(ParentProfile))]
    public Guid ParentProfileId { get; set; }

    // Bắt buộc phải gắn với 1 ParentProfile
    public required ParentProfile ParentProfile { get; set; }
}
