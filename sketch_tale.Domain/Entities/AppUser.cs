using Microsoft.AspNetCore.Identity;

namespace sketch_tale.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DOB { get; set; }
    public Boolean? Gender { get; set; }
    public DateTime? LastLoginAt { get; set; }

    //base entity
    public DateTime CreatedAt { get; set; }
    public Guid CreateBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdateBy { get; set; }
}