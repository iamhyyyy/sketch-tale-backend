namespace sketch_tale.Domain.Enums;

public enum CommonStatus
{
    Draft,
    Inactive,
    Active,
    Pending,
    Archived
}

public enum TargetAgeGroup
{
    Ag6to7,
    Age8to10,
    Age11to12
}

public enum ReadingLevel
{
    Beginner,
    Intermediate,
    Advanced
}

public enum ParentApprovalStatus
{
    NotRequired,
    Pending,
    Approved,
    Rejected
}
public enum DrawingType
{
    Canvas,
    Uploaded
}
public enum AIGenStatus
{
    Pending,
    Processing,
    Success,
    Failed
}
public enum CharacterStatus
{
    Preview,
    Pending,
    Approved,
    Rejected,
    Hidden
}
public enum GeneratedStoryStatus
{
    InProgress,
    Completed,
    Hidden
}

public enum StoryRoleType
{
    MainCharacter = 0,
    Companion = 1,
    Supporting = 2,
    Other = 3
}

public enum SubscriptionStatus
{
    PendingPayment,
    Active,
    Expired,
    Cancelled
}

public enum PaymentStatus
{
    Pending,
    Success,
    Failed
}

public enum MatchType
{
    Exact,
    Contains,
    Regex
}

public enum SeverityLevel
{
    Low,
    Medium,
    High,
    BlockImmediately
}

public enum ModerationAction
{
    Reject,
    FlagForReview,
    Mask
}

public enum ModerationStatus
{
    Pending,
    Approved,
    Rejected,
    Resolved
}


public enum ContentReportStatus
{
    Pending,
    Processing,
    Resolve,
    Dismissed
}

public enum ReportTargetType
{
    Character,
    Story,
    Content
}

public enum AIActionType
{
    ClassifyImage,
    CharacterGenerate
}

public enum CreditActionType
{
    ImageClassification = 1,  // 30 credits (Phân loại tranh)
    CharacterGeneration = 2,  // 150 credits (Tạo nhân vật)
    CharacterRegeneration = 3,// 50 credits (Tạo lại nhân vật)
    StoryExport = 4           // 200 credits (Xuất video/file truyện)
}