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
    private readonly IAuditDbContext _auditContext;

    protected AuditIdentityDbContext(DbContextOptions options) : base(options)
    {
        _auditContext = new DefaultAuditContext(this);
        _helper.SetConfig(_auditContext);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        return _helper.SaveChanges(_auditContext, () => base.SaveChanges(acceptAllChangesOnSuccess));
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        return _helper.SaveChangesAsync(_auditContext, () => base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken), cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);
    }
}
