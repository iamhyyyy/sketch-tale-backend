using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Common;
using sketch_tale.Domain.Entities;

namespace sketch_tale.Infrastructure.Data;

public class AppDbContext : AuditIdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<ChildProfile> ChildProfiles => Set<ChildProfile>();
    public DbSet<ParentProfile> ParentProfiles => Set<ParentProfile>();
    public DbSet<ParentProfileSub> ParentProfileSubs => Set<ParentProfileSub>();

    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    public DbSet<Drawing> Drawings => Set<Drawing>();
    public DbSet<CharType> CharTypes => Set<CharType>();
    public DbSet<Character> Characters => Set<Character>(); 

    public DbSet<EducationTheme> EducationThemes => Set<EducationTheme>();
    public DbSet<StoryTemplate> StoryTemplates => Set<StoryTemplate>();
    public DbSet<StoryRoleTemplate> StoryRoleTemplates => Set<StoryRoleTemplate>();
    public DbSet<StoryPageTemplate> StoryPageTemplates => Set<StoryPageTemplate>();
    public DbSet<CharacterSlotTemplate> CharacterSlotTemplates => Set<CharacterSlotTemplate>();
    public DbSet<VocabularyTemplate> VocabularyTemplates => Set<VocabularyTemplate>();
    public DbSet<StoryQuizTemplate> StoryQuizTemplates => Set<StoryQuizTemplate>();

    public DbSet<GeneratedStory> GeneratedStories => Set<GeneratedStory>();
    public DbSet<UserStoryCharacterMapping> UserStoryCharacterMappings => Set<UserStoryCharacterMapping>();
    public DbSet<ChildVocabularyProgress> ChildVocabularyProgresses => Set<ChildVocabularyProgress>();
    public DbSet<ReadingLog> ReadingLogs => Set<ReadingLog>();
    public DbSet<ChildDailyUsageLog> ChildDailyUsageLogs => Set<ChildDailyUsageLog>();
    public DbSet<ChildQuizAnswer> ChildQuizAnswers => Set<ChildQuizAnswer>();

    public DbSet<RestrictedKeyword> RestrictedKeywords => Set<RestrictedKeyword>();
    public DbSet<ProhibitedTheme> ProhibitedThemes => Set<ProhibitedTheme>();
    public DbSet<ContentReport> ContentReports => Set<ContentReport>();
    public DbSet<AIUsageLog> AIUsageLogs => Set<AIUsageLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Đổi tên bảng Identity mặc định, không override property
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        // Seed roles với GUID cố định
        var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var contentManagerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var parentRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var childRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");


        modelBuilder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid> { Id = adminRoleId, Name = "admin", NormalizedName = "ADMIN" },
            new IdentityRole<Guid> { Id = contentManagerRoleId, Name = "content manager", NormalizedName = "CONTENT MANAGER" },
            new IdentityRole<Guid> { Id = parentRoleId, Name = "parent", NormalizedName = "PARENT" },
            new IdentityRole<Guid> { Id = childRoleId, Name = "child", NormalizedName = "CHILD" }
        );

    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        var currentTime = DateTime.UtcNow.AddHours(7);
        var currentUserId = Guid.TryParse(_currentUserService.UserId, out var parsedUserId)
            ? parsedUserId
            : Guid.Empty;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Metadata.FindProperty(nameof(BaseEntity.CreatedAt)) is not null)
                    entry.Property(nameof(BaseEntity.CreatedAt)).CurrentValue = currentTime;

                if (entry.Metadata.FindProperty(nameof(BaseEntity.CreateBy)) is not null)
                {
                    var createBy = entry.Property(nameof(BaseEntity.CreateBy));
                    if (createBy.CurrentValue is Guid existing && existing == Guid.Empty)
                        createBy.CurrentValue = currentUserId;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Metadata.FindProperty(nameof(BaseEntity.UpdatedAt)) is not null)
                    entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = currentTime;

                if (entry.Metadata.FindProperty(nameof(BaseEntity.UpdateBy)) is not null)
                    entry.Property(nameof(BaseEntity.UpdateBy)).CurrentValue = currentUserId;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    
}
