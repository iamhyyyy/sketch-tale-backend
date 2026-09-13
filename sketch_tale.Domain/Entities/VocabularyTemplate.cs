using sketch_tale.Domain.Common;

namespace sketch_tale.Domain.Entities;

public class VocabularyTemplate : BaseEntity
{
    public Guid StoryPageId { get; set; }

    public string Word { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }

    // Navigation Properties
    public StoryPageTemplate StoryPage { get; set; } = null!;
}
