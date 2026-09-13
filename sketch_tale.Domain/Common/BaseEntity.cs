namespace sketch_tale.Domain.Common;

public abstract class BaseEntityWithoutId
{
    public DateTime CreatedAt { get; set; }
    public Guid CreateBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdateBy { get; set; }
}


public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreateBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdateBy { get; set; }
}