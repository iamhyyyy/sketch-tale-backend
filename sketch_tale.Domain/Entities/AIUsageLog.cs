
using sketch_tale.Domain.Common;

namespace sketch_tale.Domain.Entities;

public class AIUsageLog : BaseEntity
{
    public string Provider { get; set; } = null!;
    public string ActionType { get; set; } = null!;
    public int TokenUsed { get; set; }
    public float CostAmount { get; set; }
}
