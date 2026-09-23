namespace sketch_tale.Domain.Enums;

public enum CommonStatus
{
    Inactive,
    Active,
    Pending,
    Archived
}

public enum TargetAgeGroup
{
    Age3To5,
    Age6To8
}

public enum ParentApprovalStatus
{
    Pending,
    Approved,
    Rejected
}
public enum DrawingType
{
    Canvas,
    Uploaded
}
public enum BillingCycle
{
    Monthly = 0,
    Yearly = 1
}

public enum SubscriptionStatus
{
    PendingPayment = 0,
    Active = 1,
    Expired = 2,
    Cancelled = 3
}

public enum PaymentStatus
{
    Pending = 0,
    Success = 1,
    Failed = 2
}

public enum MatchType
{
    Exact = 1,
    Contains = 2,
    Regex = 3
}

public enum SeverityLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    BlockImmediately = 4
}

public enum ModerationAction
{
    Reject = 1,
    FlagForReview = 2,
    Mask = 3
}

public enum FlaggedBy
{
    SystemFilter = 1,
    ParentReport = 2,
    AdminCheck = 3
}

public enum ModerationStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Resolved = 4
}