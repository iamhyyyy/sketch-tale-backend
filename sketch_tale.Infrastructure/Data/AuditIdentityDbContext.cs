using Audit.EntityFramework;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace sketch_tale.Infrastructure.Data;

// Lớp trung gian giúp kết nối AuditDbContext và IdentityDbContext
public abstract class AuditIdentityDbContext<TUser, TRole, TKey> : IdentityDbContext<TUser, TRole, TKey>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    private readonly DbContextHelper _helper = new DbContextHelper();

    protected AuditIdentityDbContext(DbContextOptions options) : base(options)
    {
        _helper.SetConfig((IAuditDbContext)this);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _helper.SaveChangesAsync((IAuditDbContext)this, () => base.SaveChangesAsync(cancellationToken));
    }
}