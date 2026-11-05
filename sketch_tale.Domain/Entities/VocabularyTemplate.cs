using sketch_tale.Domain.Common;

namespace sketch_tale.Domain.Entities;

public class VocabularyTemplate : BaseEntity
{
    public Guid StoryPageTemplateId { get; set; }

    public string WordVi { get; set; } = null!;
    public string WordEn { get; set; } = string.Empty;
    public string Meaning { get; set; } = null!;
    public string IPA { get; set; } = string.Empty;
    public string AudioUrlVi { get; set; } = string.Empty;
    public string AudioUrlEn { get; set; } = string.Empty;
}
