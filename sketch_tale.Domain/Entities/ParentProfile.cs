using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class ParentProfile : BaseEntity
{
    public Guid UserId { get; set; }

    public int ChildProfileLimit { get; set; } = 1;
    public int RemainingChild { get; set; } = 1;

    public int CharacterLimit { get; set; } = 5;
    public int RemainingCharacters { get; set; } = 5;

    public int ExportStoryLimit { get; set; } = 0;
    public bool RemainingExport { get; set; } = false;

    public bool AccessFullStories { get; set; } = false;

    public ParentProfileSub? ParentProfileSub { get; set; }
}
