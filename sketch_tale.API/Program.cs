using Audit.Core;
using Audit.EntityFramework;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Application.Mappings;
using sketch_tale.Application.Services;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Interfaces;
using sketch_tale.Infrastructure.Data;
using sketch_tale.Infrastructure.Repositories;
using sketch_tale.Infrastructure.Services;
using sketch_tale.Infrastructure.Settings;
using SmartCarWash.Application.Services;

namespace sketch_tale.API;

public class Program
{
    public static async Task Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        //dùng để lưu log
        Audit.Core.Configuration.DataProvider = new Audit.EntityFramework.Providers.EntityFrameworkDataProvider
        {
            AuditEntityAction = (evt, entry, auditEntity) =>
            {
                if (auditEntity is AuditLog auditLog)
                {
                    //auditLog.Id = Guid.NewGuid();

                    // Lấy UserId từ environment hoặc gán Guid.Empty nếu chưa có
                    auditLog.UserId = Guid.TryParse(evt.Environment?.UserName, out var parsedUser)
                        ? parsedUser
                        : Guid.Empty;

                    auditLog.Action = entry.Action; // "Insert", "Update", "Delete"
                    auditLog.EntityName = entry.EntityType.Name; // Tên bảng/Entity (VD: ChildProfile)

                    // Lấy Primary Key của bản ghi bị tác động
                    var pk = entry.PrimaryKey.Values.FirstOrDefault();
                    auditLog.PrimaryKey = pk != null && Guid.TryParse(pk.ToString(), out var parsedPk)
                        ? parsedPk
                        : Guid.Empty;

                    // Map giá trị cũ, mới và các cột bị sửa đổi
                    auditLog.OldValues = entry.Changes != null ? System.Text.Json.JsonSerializer.Serialize(entry.ColumnValues) : null;
                    auditLog.NewValues = entry.ToJson();
                    auditLog.ChangedColumns = entry.Changes != null
                        ? string.Join(", ", entry.Changes.Select(c => c.ColumnName))
                        : null;

                    //auditLog.Timestamp = DateTime.UtcNow.AddHours(7);
                }

                return Task.FromResult(true);
            }
        };

        // Đăng ký Unit of Work
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Đăng ký Repository
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

        // Đăng ký Service system
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();

        // Đăng ký Email Service
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.Configure<SendGridSettings>(builder.Configuration.GetSection("SendGridSettings"));

        // Add services to the container.
        builder.Services.AddControllers();

        //Add db
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        //Add Identity services, chỗ này là đăng ký để ASP.Net tự DI dùm ở chỗ AuthService
        builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>()
                        .AddEntityFrameworkStores<AppDbContext>()
                        .AddDefaultTokenProviders();

        //Add AutoMapper
        builder.Services.AddAutoMapper(typeof(MappingProfile));

        builder.Services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        //seed data
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<AppDbContext>();

                // 1. Tự động chạy Migration (Tạo bảng trên Neon nếu chưa có)
                await context.Database.MigrateAsync();

                // 2. Chạy SeedData
                var seedData = new SeedData();
                await seedData.InitializeAsync(services);

                Console.WriteLine("Database Migration & Seed completed successfully!");
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            }
        }

        // Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        //{
        //    app.UseSwagger();
        //    app.UseSwaggerUI();
        //}
        app.UseSwagger();
        app.UseSwaggerUI();
        //app.UseCors("AllowAll");
        app.UseHttpsRedirection();

        //app.UseAuthentication();
        //app.UseAuthorization();
        app.MapControllers();

        app.MapGet("/", context =>
        {
            context.Response.Redirect("/swagger");
            return Task.CompletedTask;
        });

        //environment variable for port, default to 8080 if not set
        var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        app.Run($"http://0.0.0.0:{port}");

        //chạy test local thì dùng cái này cho nhanh, chạy trên server thì dùng cái trên
        //app.Run();

    }
}
