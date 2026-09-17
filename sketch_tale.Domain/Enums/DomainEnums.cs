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