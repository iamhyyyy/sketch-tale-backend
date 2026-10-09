
using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class AIUsageLog : BaseEntity
{
    public string Provider { get; set; } = null!;
    public AIActionType ActionType { get; set; }
    public int TokenUsed { get; set; }
    public float CostAmount { get; set; }
}
