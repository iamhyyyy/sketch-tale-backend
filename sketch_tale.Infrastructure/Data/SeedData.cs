using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using sketch_tale.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Infrastructure.Data;

public class SeedData
{
    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

        try
        {
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }

            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager, context);

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
        // Nếu đã có bất kỳ role nào trong hệ thống -> Bỏ qua
        if (await roleManager.Roles.AnyAsync()) return;

        foreach (var role in new[] { "admin", "content manager", "parent", "children" })
        {
            await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role, NormalizedName = role.ToUpper() });
        }
    }

    private static async Task SeedUsersAsync(UserManager<AppUser> userManager, AppDbContext context)
    {
        // Nếu bảng Users đã có data -> Bỏ qua không tạo user lẫn profile nữa
        if (await userManager.Users.AnyAsync()) return;

        await CreateUserAsync(userManager, "admin", "admin@sketch.tale.com", "admin123", "Admin", "User", "admin");
        await CreateUserAsync(userManager, "cmanager", "cmanager@sketch.tale.com", "cmanager123", "Manager", "User", "manager");
        await CreateUserAsync(userManager, "customer", "parent@sketch.tale.com", "parent123", "Parent", "User", "parent");
    }

    private static async Task CreateUserAsync(UserManager<AppUser> userManager, string username, string email, string password, string firstName, string lastName, string role)
    {
        if (await userManager.FindByNameAsync(username) == null)
        {
            var user = new AppUser
            {
                UserName = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }

}
