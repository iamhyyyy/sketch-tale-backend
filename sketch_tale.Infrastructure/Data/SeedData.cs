using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Infrastructure.Data;

public class SeedData
{
    private static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-000000000000");

    private static DateTime SeedTime => DateTime.UtcNow.AddHours(7);

    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        try
        {
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }

            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);
            await SeedSubscriptionPlanAsync(context);

            Console.WriteLine("All Seed Completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during database seeding: {ex.Message}");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        if (await roleManager.Roles.AnyAsync()) return;

        foreach (var role in new[] { "admin", "content manager", "parent", "child" })
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>
            {
                Name = role,
                NormalizedName = role.ToUpperInvariant()
            });
        }
    }

    private static async Task SeedUsersAsync(UserManager<User> userManager)
    {
        // Nếu bảng Users đã có data -> Bỏ qua không tạo user
        if (await userManager.Users.AnyAsync()) return;

        await CreateUserAsync(userManager, "admin", "admin@sketch.tale.com", "admin123", "Admin", "User", "admin");
        await CreateUserAsync(userManager, "cmanager", "cmanager@sketch.tale.com", "cmanager123", "Content", "Manager", "content manager");
        await CreateUserAsync(userManager, "parent", "parent@sketch.tale.com", "parent123", "Parent", "User", "parent");
        await CreateUserAsync(userManager, "child", "child@sketch.tale.com", "child123", "Little", "Artist", "child");
    }

    private static async Task CreateUserAsync(
        UserManager<User> userManager,
        string username,
        string email,
        string password,
        string firstName,
        string lastName,
        string role)
    {
        if (await userManager.FindByNameAsync(username) != null) return;

        var user = new User
        {
            UserName = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
            CreatedAt = SeedTime,
            CreateBy = SystemUserId
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
        else
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            Console.WriteLine($"Failed to create user '{username}': {errors}");
        }
    }

    private static async Task SeedSubscriptionPlanAsync(AppDbContext context)
    {
        if (await context.SubscriptionPlans.AnyAsync()) return;

        var now = SeedTime;
        context.SubscriptionPlans.AddRange(
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Free",
                Price = 0,

                ChildProfileLimit = 1,
                CharacterLimit = 5,
                ExportStoryLimit = 0,
                AccessFullStories = false,

                DurationDays = 30,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Plus",
                Price = 49,

                ChildProfileLimit = 3,
                CharacterLimit = 30,
                ExportStoryLimit = 5,
                AccessFullStories = true,

                DurationDays = 30,
                FreeTrialDays = 7,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Pro",
                Price = 99,

                ChildProfileLimit = 5,
                CharacterLimit = 100,
                ExportStoryLimit = 20,
                AccessFullStories = true,

                DurationDays = 30,
                FreeTrialDays = 7,
                CreatedAt = now,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    
  
}
