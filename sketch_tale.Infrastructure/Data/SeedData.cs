using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Infrastructure.Data;

public class SeedData
{
    private static readonly Guid SystemUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

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
            await SeedCreditCostAsync(context);

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

        foreach (var role in new[] { "system", "admin", "content manager", "parent", "child" })
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

        await CreateUserAsync(userManager, "admin", "admin@sketch.tale.com", "Admin@123", "Admin", "User", "admin");
        await CreateUserAsync(userManager, "cmanager", "cmanager@sketch.tale.com", "Manager@123", "Content", "Manager", "content manager");
        await CreateUserAsync(userManager, "parent", "parent@sketch.tale.com", "Parent@123", "Parent", "User", "parent");
        await CreateUserAsync(userManager, "child", "child@sketch.tale.com", "Child@123", "Little", "Artist", "child");
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

        //var now = SeedTime;
        context.SubscriptionPlans.AddRange(
            new SubscriptionPlan
            {
                Name = "Free",
                Price = 0,
                Code = "FreePlan",

                ChildProfileLimit = 1,
                MonthlyCreditLimit = 500,
                AccessFullStories = false,
                CanExportStory = false,

                DurationDays = 9999,
                CreateBy = SystemUserId
            },
            new SubscriptionPlan
            {
                Name = "Plus",
                Price = 49,
                Code = "PlusPlan",

                ChildProfileLimit = 3,
                MonthlyCreditLimit = 4000,
                AccessFullStories = true,
                CanExportStory = true,


                DurationDays = 30,
                FreeTrialDays = 7,
                CreateBy = SystemUserId
            },
            new SubscriptionPlan
            {
                Name = "Pro",
                Price = 99,
                Code = "ProPlan",

                ChildProfileLimit = 5,
                MonthlyCreditLimit = 10000,
                AccessFullStories = true,
                CanExportStory = true,

                DurationDays = 30,
                FreeTrialDays = 0,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }


    private static async Task SeedCreditCostAsync(AppDbContext context)
    {
        if (await context.CreditCosts.AnyAsync()) return;

        //var now = SeedTime;
        context.CreditCosts.AddRange(
            new CreditCost
            {
                FeatureKey = "ImageClassification",
                Cost = 30,
                FeatureNameVi = "Phân loại hình ảnh",
                FeatureNameEn = "Image Classification",

                IsActive = true,
                CreateBy = SystemUserId
            },
            new CreditCost
            {
                FeatureKey = "CharacterGeneration",
                Cost = 150,
                FeatureNameVi = "Tạo nhân vật",
                FeatureNameEn = "Character Generation",

                IsActive = true,
                CreateBy = SystemUserId
            },
            new CreditCost
            {
                FeatureKey = "CharacterRegeneration",
                Cost = 50,
                FeatureNameVi = "Tạo lại nhân vật",
                FeatureNameEn = "Character Regeneration",

                IsActive = true,
                CreateBy = SystemUserId
            },
            new CreditCost
            {
                FeatureKey = "CharacterRegeneration",
                Cost = 50,
                FeatureNameVi = "Tạo lại nhân vật",
                FeatureNameEn = "Character Regeneration",

                IsActive = true,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }
}
