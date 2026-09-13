using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class StoryPageTemplate : BaseEntity
{
    public Guid StoryTemplateId { get; set; }

    public int PageNumber { get; set; }
    public string RawText { get; set; } = string.Empty;
    public string BackgroundUrl { get; set; } = string.Empty;
    public string? AudioNarrationUrl { get; set; }
    public CommonStatus Status { get; set; }

    // Navigation Properties
    public StoryTemplate StoryTemplate { get; set; } = null!;
    public ICollection<CharacterSlotTemplate> CharacterSlotTemplates { get; set; } = new List<CharacterSlotTemplate>();
    public ICollection<VocabularyTemplate> VocabularyTemplates { get; set; } = new List<VocabularyTemplate>();
}
