using Audit.Core;
using Audit.EntityFramework;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
using System.IdentityModel.Tokens.Jwt;
using System.Text;

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
                    auditLog.CreateBy = Guid.TryParse(evt.Environment?.UserName, out var parsedUser)
                        ? parsedUser
                        : Guid.Empty;

                    auditLog.Action = entry.Action; // "Insert", "Update", "Delete"
                    auditLog.EntityName = entry.EntityType.Name; // Tên bảng/Entity (VD: ChildProfile)

                    // Lấy Primary Key của bản ghi bị tác động
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
                }

                return Task.FromResult(true);
            }
        };

        // Đăng ký Unit of Work
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Đăng ký Repository
        builder.Services.AddScoped<IEducationThemeRepository, EducationThemeRepository>();
        builder.Services.AddScoped<IStoryTemplateRepository, StoryTemplateRepository>();
        builder.Services.AddScoped<IStoryPageTemplateRepository, StoryPageTemplateRepository>();    
        builder.Services.AddScoped<IStoryRoleTemplateRepository, StoryRoleTemplateRepository>();    

        // Đăng ký Service system
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddScoped<IEducationThemeService, EducationThemeService>();
        builder.Services.AddScoped<IStoryTemplateService, StoryTemplateService>();
        builder.Services.AddScoped<IStoryPageTemplateService, StoryPageTemplateService>();
        builder.Services.AddScoped<IStoryRoleTemplateService, StoryRoleTemplateService>();
        // Cloudinary image upload service
        builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
        // Đăng ký Email Service
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.Configure<SendGridSettings>(builder.Configuration.GetSection("SendGridSettings"));
        // Đăng ký Auth Service
        // Đăng ký AuthService và JwtService vào DI container
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IJwtService, JwtService>();
        // Add services to the container.
        builder.Services.AddControllers();

        //Add db
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        //Add Identity services, chỗ này là đăng ký để ASP.Net tự DI dùm ở chỗ AuthService
        builder.Services.AddIdentity<User, IdentityRole<Guid>>()
                        .AddEntityFrameworkStores<AppDbContext>()
                        .AddDefaultTokenProviders();

        //Add AutoMapper
        builder.Services.AddAutoMapper(typeof(MappingProfile));

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "SketchTale API", Version = "v1" });

            // Cấu hình nút Authorize (Ổ khóa) trên Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Chỉ cần dán trực tiếp JWT Token của bạn vào ô dưới đây (Không cần gõ chữ Bearer)",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
        });
        // 2. Cấu hình JWT Authentication (Sửa lại cho khớp với JwtSettings và Secret trong appsettings.json)
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Secret"];

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false, // Vì JwtSettings của bạn không cấu hình Issuer/Audience riêng
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey ?? string.Empty)),
                NameClaimType = JwtRegisteredClaimNames.Sub,
                RoleClaimType = "role",
                ClockSkew = TimeSpan.Zero
            };

        });
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
        app.UseCors("AllowAll");
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();
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
