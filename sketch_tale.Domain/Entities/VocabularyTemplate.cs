using sketch_tale.Domain.Common;

namespace sketch_tale.Domain.Entities;

public class VocabularyTemplate : BaseEntity
{
    public Guid StoryPageTemplateId { get; set; }

    public string Word { get; set; } = null!;
    public string Meaning { get; set; } = null!;
    public string AudioUrl { get; set; } = string.Empty;
}
