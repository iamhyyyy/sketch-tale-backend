using sketch_tale.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sketch_tale.Domain.Entities;

public class ParentProfileSub : BaseEntityWithoutId
{
    [Key]
    [ForeignKey(nameof(User))]
    public Guid ParentProfileSubId { get; set; }

    
    [ForeignKey(nameof(ParentProfile))]
    public Guid ParentProfileId { get; set; }

    public virtual User User { get; set; } = null!;
    public ParentProfile ParentProfile { get; set; } = null!;
}
