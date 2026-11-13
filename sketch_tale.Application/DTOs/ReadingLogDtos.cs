using System;

namespace sketch_tale.Application.DTOs;

public class StartReadingLogDto
{
    public Guid GeneratedStoryId { get; set; }
}

public class UpdateReadingProgressDto
{
    public int CurrentPage { get; set; }
    public int ReadDurationSeconds { get; set; }
}

public class ReadingLogDto : BaseFieldDto
{
    public Guid ChildProfileId { get; set; }
    public Guid GeneratedStoryId { get; set; }
    public Guid StoryTemplateId { get; set; }

    public string StoryTitle { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public int TotalPageNumbers { get; set; }

    public int CurrentPage { get; set; }
    public int ReadDurationSeconds { get; set; }
    public bool IsCompleted { get; set; }
}

public class CompletedReadingLogDto : BaseFieldDto
{
    public Guid ChildProfileId { get; set; }
    public Guid GeneratedStoryId { get; set; }
    public Guid StoryTemplateId { get; set; }

    public string StoryTitle { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public int TotalPageNumbers { get; set; }
    public int LastPageRead { get; set; }
    public bool IsFavorite { get; set; }

    public int ReadDurationSeconds { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CompletedAt { get; set; }
}

