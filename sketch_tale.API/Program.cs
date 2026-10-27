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
            // Cần thiết: Ánh xạ mọi entity thay đổi sang AuditLog (trừ chính AuditLog)
            AuditTypeMapper = (entityType, entry) =>
                entityType == typeof(AuditLog) ? null : typeof(AuditLog),

            AuditEntityAction = (evt, entry, auditEntity) =>
            {
                if (auditEntity is AuditLog auditLog)
                {
                    auditLog.Id = Guid.NewGuid();

                    auditLog.CreatedAt = DateTime.UtcNow.AddHours(7);
                    auditLog.CreateBy = Guid.Empty; // Để Guid.Empty để SaveChangesAsync tự gán currentUserId
                    auditLog.UpdatedAt = null;      // Bản ghi Log mới tạo không có UpdateAt
                    auditLog.UpdateBy = null;       // Bản ghi Log mới tạo không có UpdateBy

                    auditLog.AuditAction = entry.Action; // "Insert", "Update", "Delete"
                    auditLog.EntityName = entry.EntityType.Name;

                    var pk = entry.PrimaryKey.Values.FirstOrDefault();
                    auditLog.PrimaryKey = pk != null && Guid.TryParse(pk.ToString(), out var parsedPk)
                        ? parsedPk
                        : Guid.Empty;

                    if (entry.Changes != null && entry.Changes.Count > 0)
                    {
                        bool AreValuesEqual(object val1, object val2)
                        {
                            if (val1 == null && val2 == null) return true;
                            if (val1 == null || val2 == null) return false;

                            // So sánh chuỗi representation đề phòng khác kiểu dữ liệu nhưng cùng giá trị (vd: int vs long, DateTime formatting)
                            return Equals(val1, val2) || string.Equals(val1.ToString(), val2.ToString());
                        }

                        // Danh sách các cột muốn bỏ qua không audit (nếu cần)
                        var ignoredColumns = new HashSet<string> { "UpdatedAt", "UpdatedBy" };

                        // Lọc riêng từng cột: Chỉ lấy những cột mà OriginalValue KHÁC NewValue
                        var realChanges = entry.Changes
                            .Where(c => !ignoredColumns.Contains(c.ColumnName))
                            .Where(c => !AreValuesEqual(c.OriginalValue, c.NewValue))
                            .ToList();

                        if (realChanges.Count > 0)
                        {
                            var oldValuesDict = realChanges.ToDictionary(c => c.ColumnName, c => c.OriginalValue);
                            var newValuesDict = realChanges.ToDictionary(c => c.ColumnName, c => c.NewValue);

                            auditLog.OldValues = System.Text.Json.JsonSerializer.Serialize(oldValuesDict);
                            auditLog.NewValues = System.Text.Json.JsonSerializer.Serialize(newValuesDict);
                            auditLog.ChangedColumns = string.Join(", ", realChanges.Select(c => c.ColumnName));
                        }
                        else
                        {
                            // Nếu sau khi lọc không có cột nào thực sự đổi value
                            auditLog.OldValues = null;
                            auditLog.NewValues = null;
                            auditLog.ChangedColumns = null;
                        }
                    }
                    else
                    {
                        // Xử lý Insert / Delete
                        auditLog.OldValues = entry.Action == "Delete" ? entry.ToJson() : null;
                        auditLog.NewValues = entry.Action == "Insert" ? entry.ToJson() : null;
                        auditLog.ChangedColumns = null;
                    }
                }

                return Task.FromResult(true);
            }
        };


        // Đăng ký Unit of Work
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Đăng ký Repository
        builder.Services.AddScoped<IEducationThemeRepository, EducationThemeRepository>();
        builder.Services.AddScoped<ICreditCostRepository, CreditCostRepository>();
        builder.Services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        builder.Services.AddScoped<IRestrictedKeywordRepository, RestrictedKeywordRepository>();
        builder.Services.AddScoped<IProhibitedThemeRepository, ProhibitedThemeRepository>();
        builder.Services.AddScoped<IContentReportRepository, ContentReportRepository>();
        builder.Services.AddScoped<ICharTypeRepository, CharTypeRepository>();
        builder.Services.AddScoped<IUserStoryCharacterMappingRepository, UserStoryCharacterMappingRepository>();
        builder.Services.AddScoped<IStoryTemplateRepository, StoryTemplateRepository>();
        builder.Services.AddScoped<IStoryPageTemplateRepository, StoryPageTemplateRepository>();    
        builder.Services.AddScoped<IStoryRoleTemplateRepository, StoryRoleTemplateRepository>();    
        builder.Services.AddScoped<ICharacterSlotTemplateRepository, CharacterSlotTemplateRepository>();
        builder.Services.AddScoped<IStoryQuizTemplateRepository, StoryQuizTemplateRepository>();
        builder.Services.AddScoped<IVocabularyTemplateRepository, VocabularyTemplateRepository>();
        builder.Services.AddScoped<IParentProfileRepository, ParentProfileRepository>();
        builder.Services.AddScoped<IChildProfileRepository, ChildProfileRepository>();

        // Đăng ký Service system
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddScoped<IEducationThemeService, EducationThemeService>();
        builder.Services.AddScoped<ICreditCostService, CreditCostService>();
        builder.Services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
        builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
        builder.Services.AddScoped<IAuditLogService, AuditLogService>();
        builder.Services.AddScoped<IRestrictedKeywordService, RestrictedKeywordService>();
        builder.Services.AddScoped<IProhibitedThemeService, ProhibitedThemeService>();
        builder.Services.AddScoped<IContentReportService, ContentReportService>();
        builder.Services.AddScoped<ICharTypeService, CharTypeService>();
        builder.Services.AddScoped<IUserStoryCharacterMappingService, UserStoryCharacterMappingService>();
        builder.Services.AddScoped<IStoryTemplateService, StoryTemplateService>();
        builder.Services.AddScoped<IStoryPageTemplateService, StoryPageTemplateService>();
        builder.Services.AddScoped<IStoryRoleTemplateService, StoryRoleTemplateService>();
        builder.Services.AddScoped<ICharacterSlotTemplateService, CharacterSlotTemplateService>();
        builder.Services.AddScoped<IStoryQuizTemplateService, StoryQuizTemplateService>();
        builder.Services.AddScoped<IVocabularyTemplateService, VocabularyTemplateService>();
        builder.Services.AddScoped<IParentProfileService, ParentProfileService>();
        builder.Services.AddScoped<IChildProfileService, ChildProfileService>();
        // Cloudinary image upload service
        builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

        // Đăng ký Email Service
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.Configure<SendGridSettings>(builder.Configuration.GetSection("SendGridSettings"));

        // Đăng ký Memory Cache & Token Blacklist
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();

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
                ClockSkew = TimeSpan.Zero,
                LifetimeValidator = (notBefore, expires, securityToken, validationParameters) =>
                {
                    if (expires != null && DateTime.UtcNow > expires)
                    {
                        var accessor = new HttpContextAccessor();
                        var path = accessor.HttpContext?.Request.Path.Value;
                        if (path != null && path.EndsWith("/refresh-token", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        return false;
                    }
                    return true;
                }
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var blacklistService = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();

                    // 1. Kiểm tra qua JTI claim
                    var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                    if (!string.IsNullOrEmpty(jti) && await blacklistService.IsJtiBlacklistedAsync(jti))
                    {
                        context.Fail("Token này đã bị vô hiệu hóa do người dùng đã đăng xuất.");
                        return;
                    }

                    // 2. Kiểm tra qua raw token trong Authorization header
                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        var rawToken = authHeader.Substring("Bearer ".Length).Trim();
                        if (await blacklistService.IsTokenBlacklistedAsync(rawToken))
                        {
                            context.Fail("Token này đã bị vô hiệu hóa do người dùng đã đăng xuất.");
                            return;
                        }
                    }
                },
                OnChallenge = async context =>
                {
                    if (!context.Response.HasStarted)
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        var msg = context.ErrorDescription ?? "Bạn chưa đăng nhập hoặc token đã hết hạn / bị vô hiệu hóa.";
                        var json = System.Text.Json.JsonSerializer.Serialize(new { message = msg });
                        await context.Response.WriteAsync(json);
                    }
                }
            };

        });
        var app = builder.Build();

        //seed data
        Audit.Core.Configuration.AuditDisabled = true; //Tắt tạm thời Audit.NET
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
        Audit.Core.Configuration.AuditDisabled = false;//Bật lại Audit.NET

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
