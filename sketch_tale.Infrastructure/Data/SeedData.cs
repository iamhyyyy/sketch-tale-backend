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
            await SeedDemoUsersAsync(userManager);
            await SeedCharTypesAsync(context);
            await SeedEducationThemesAsync(context);
            await SeedRestrictedKeywordsAsync(context);
            await SeedProhibitedThemesAsync(context);
            await SeedParentProfilesAsync(context, userManager);
            await SeedParentProfileSubsAsync(context);
            await SeedChildProfilesAsync(context, userManager);
            await SeedSubscriptionsAsync(context);
            await SeedPaymentTransactionsAsync(context);
            await SeedCreditTransactionsAsync(context);
            await SeedStoryTemplatesAsync(context);
            await SeedStoryRoleTemplatesAsync(context);
            await SeedStoryPageTemplatesAsync(context);
            await SeedCharacterSlotTemplatesAsync(context);
            await SeedVocabularyTemplatesAsync(context);
            await SeedStoryQuizTemplatesAsync(context);
            await SeedDrawingsAsync(context);
            await SeedAICharacterGeneratesAsync(context);
            await SeedCharactersAsync(context);
            await SeedGeneratedStoriesAsync(context);
            await SeedUserStoryCharacterMappingsAsync(context);
            await SeedChildVocabularyProgressesAsync(context);
            await SeedReadingLogsAsync(context);
            await SeedChildDailyUsageLogsAsync(context);
            await SeedChildQuizAnswersAsync(context);
            await SeedContentReportsAsync(context);
            await SeedAIUsageLogsAsync(context);
            await SeedAuditLogsAsync(context);

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
        await CreateUserAsync(userManager, "manager", "manager@sketch.tale.com", "Manager@123", "Content", "Manager", "content manager");
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
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Name = "Free",
                Price = 0,
                Code = "FreePlan",

                ChildProfileLimit = 1,
                MonthlyCreditLimit = 500,
                AccessFullStories = false,
                CanExportStory = false,
                HaveParentProfileSub = false,

                DurationDays = 9999,
                CreateBy = SystemUserId
            },
            new SubscriptionPlan
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Name = "Plus",
                Price = 49000,
                Code = "PlusPlan",

                ChildProfileLimit = 3,
                MonthlyCreditLimit = 4000,
                AccessFullStories = true,
                CanExportStory = true,
                HaveParentProfileSub = true,


                DurationDays = 30,
                FreeTrialDays = 7,
                CreateBy = SystemUserId
            },
            new SubscriptionPlan
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Name = "Pro",
                Price = 99000,
                Code = "ProPlan",

                ChildProfileLimit = 5,
                MonthlyCreditLimit = 10000,
                AccessFullStories = true,
                CanExportStory = true,
                HaveParentProfileSub = true,

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
                FeatureKey = "ExportStory",
                Cost = 200,
                FeatureNameVi = "Xuất câu truyện",
                FeatureNameEn = "Export Story",

                IsActive = true,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedDemoUsersAsync(UserManager<User> userManager)
    {
        await CreateUserAsync(userManager, "parent2", "parent2@sketch.tale.com", "Parent@123", "Parent", "Two", "parent");
        await CreateUserAsync(userManager, "child2", "child2@sketch.tale.com", "Child@123", "Little", "Dreamer", "child");
    }

    private static async Task SeedCharTypesAsync(AppDbContext context)
    {
        if (await context.CharTypes.AnyAsync()) return;

        context.CharTypes.AddRange(
            new CharType
            {
                Name = "Animal",
                Description = "Friendly animal characters from children's drawings.",
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new CharType
            {
                Name = "Human",
                Description = "Kid heroes and everyday people.",
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new CharType
            {
                Name = "Fantasy",
                Description = "Dragons, fairies and imaginary friends.",
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedEducationThemesAsync(AppDbContext context)
    {
        if (await context.EducationThemes.AnyAsync()) return;

        context.EducationThemes.AddRange(
            new EducationTheme
            {
                Name = "Kindness",
                Description = "Stories about sharing, empathy and helping others.",
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new EducationTheme
            {
                Name = "Courage",
                Description = "Stories that help children try new things and face fears.",
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new EducationTheme
            {
                Name = "Nature",
                Description = "Stories about animals, plants and taking care of Earth.",
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedRestrictedKeywordsAsync(AppDbContext context)
    {
        if (await context.RestrictedKeywords.AnyAsync()) return;

        context.RestrictedKeywords.AddRange(
            new RestrictedKeyword
            {
                Keyword = "violence",
                Description = "Block violent wording in generated content.",
                MatchType = sketch_tale.Domain.Enums.MatchType.Contains,
                SeverityLevel = SeverityLevel.High,
                Action = ModerationAction.Reject,
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new RestrictedKeyword
            {
                Keyword = "weapon",
                Description = "Flag weapon-related terms for review.",
                MatchType = sketch_tale.Domain.Enums.MatchType.Contains,
                SeverityLevel = SeverityLevel.Medium,
                Action = ModerationAction.FlagForReview,
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new RestrictedKeyword
            {
                Keyword = "kill",
                Description = "Immediately block harmful action verbs.",
                MatchType = sketch_tale.Domain.Enums.MatchType.Exact,
                SeverityLevel = SeverityLevel.BlockImmediately,
                Action = ModerationAction.Reject,
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedProhibitedThemesAsync(AppDbContext context)
    {
        if (await context.ProhibitedThemes.AnyAsync()) return;

        context.ProhibitedThemes.AddRange(
            new ProhibitedTheme
            {
                Name = "Horror",
                Description = "Scary or nightmare themes are not allowed.",
                MatchType = sketch_tale.Domain.Enums.MatchType.Contains,
                SeverityLevel = SeverityLevel.High,
                Action = ModerationAction.Reject,
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new ProhibitedTheme
            {
                Name = "Gambling",
                Description = "Gambling or betting topics are prohibited.",
                MatchType = sketch_tale.Domain.Enums.MatchType.Contains,
                SeverityLevel = SeverityLevel.Medium,
                Action = ModerationAction.FlagForReview,
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new ProhibitedTheme
            {
                Name = "Adult content",
                Description = "Any adult-only theme must be blocked immediately.",
                MatchType = sketch_tale.Domain.Enums.MatchType.Contains,
                SeverityLevel = SeverityLevel.BlockImmediately,
                Action = ModerationAction.Reject,
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedParentProfilesAsync(AppDbContext context, UserManager<User> userManager)
    {
        if (await context.ParentProfiles.AnyAsync()) return;

        var parent = await userManager.FindByNameAsync("parent");
        var parent2 = await userManager.FindByNameAsync("parent2");
        if (parent is null || parent2 is null) return;

        context.ParentProfiles.AddRange(
            new ParentProfile
            {
                UserId = parent.Id,
                RemainingChildProfile = 2,
                RemainingMonthlyCredit = 3720,
                TrialPlusPlan = true,
                CreateBy = SystemUserId
            },
            new ParentProfile
            {
                UserId = parent2.Id,
                RemainingChildProfile = 0,
                RemainingMonthlyCredit = 470,
                TrialPlusPlan = false,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedParentProfileSubsAsync(AppDbContext context)
    {
        if (await context.ParentProfileSubs.AnyAsync()) return;

        var parents = await context.ParentProfiles.OrderBy(p => p.CreatedAt).ToListAsync();
        if (parents.Count < 2) return;

        context.ParentProfileSubs.AddRange(
            new ParentProfileSub
            {
                ParentProfileId = parents[0].Id,
                UserId = parents[0].UserId,
                ParentProfile = parents[0],
                CreateBy = SystemUserId
            },
            new ParentProfileSub
            {
                ParentProfileId = parents[1].Id,
                UserId = parents[1].UserId,
                ParentProfile = parents[1],
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedChildProfilesAsync(AppDbContext context, UserManager<User> userManager)
    {
        if (await context.ChildProfiles.AnyAsync()) return;

        var child = await userManager.FindByNameAsync("child");
        var child2 = await userManager.FindByNameAsync("child2");
        var parents = await context.ParentProfiles.OrderBy(p => p.CreatedAt).ToListAsync();
        var themes = await context.EducationThemes.OrderBy(t => t.Name).ToListAsync();
        if (child is null || child2 is null || parents.Count < 2) return;

        var allowedThemes = themes.Count >= 2
            ? $"[{string.Join(",", themes.Take(2).Select(t => $"\"{t.Id}\""))}]"
            : null;

        context.ChildProfiles.AddRange(
            new ChildProfile
            {
                UserId = child.Id,
                ParentProfileId = parents[0].Id,
                NickName = "Little Artist",
                TargetAgeGroup = TargetAgeGroup.Ag6to7,
                ReadingLevel = ReadingLevel.Beginner,
                DailyTimeLimit = 30,
                DailyCharacterLimit = 2,
                RemainingCharacterLimit = 1,
                CreateBy = SystemUserId
            },
            new ChildProfile
            {
                UserId = child2.Id,
                ParentProfileId = parents[1].Id,
                NickName = "Little Dreamer",
                TargetAgeGroup = TargetAgeGroup.Age8to10,
                ReadingLevel = ReadingLevel.Intermediate,
                DailyTimeLimit = 45,
                DailyCharacterLimit = 1,
                RemainingCharacterLimit = 0,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedSubscriptionsAsync(AppDbContext context)
    {
        if (await context.Subscriptions.AnyAsync()) return;

        var parents = await context.ParentProfiles.OrderBy(p => p.CreatedAt).ToListAsync();
        var plusPlan = await context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Code == "PlusPlan");
        var freePlan = await context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Code == "FreePlan");
        if (parents.Count < 2 || plusPlan is null || freePlan is null) return;

        var now = SeedTime;
        context.Subscriptions.AddRange(
            new Subscription
            {
                ParentProfileId = parents[0].Id,
                SubscriptionPlanId = plusPlan.Id,
                Status = SubscriptionStatus.Active,
                StartDate = now.AddDays(-10),
                EndDate = now.AddDays(20),
                CreateBy = SystemUserId
            },
            new Subscription
            {
                ParentProfileId = parents[1].Id,
                SubscriptionPlanId = freePlan.Id,
                Status = SubscriptionStatus.Active,
                StartDate = now.AddDays(-30),
                EndDate = now.AddDays(9969),
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedPaymentTransactionsAsync(AppDbContext context)
    {
        if (await context.PaymentTransactions.AnyAsync()) return;

        var subscriptions = await context.Subscriptions.OrderBy(s => s.CreatedAt).ToListAsync();
        if (subscriptions.Count < 2) return;

        var now = SeedTime;
        context.PaymentTransactions.AddRange(
            new PaymentTransaction
            {
                SubscriptionId = subscriptions[0].Id,
                ParentProfileId = subscriptions[0].ParentProfileId,
                Amount = 49,
                Status = PaymentStatus.Success,
                PaymentProvider = "VNPAY",
                TxnRef = "SEEDTXN0001",
                TransactionNo = "13800123",
                BankCode = "NCB",
                ResponseCode = "00",
                PayDate = now.AddDays(-10),
                RawIpnResponse = "{\"vnp_ResponseCode\":\"00\"}",
                CreateBy = SystemUserId
            },
            new PaymentTransaction
            {
                SubscriptionId = subscriptions[1].Id,
                ParentProfileId = subscriptions[1].ParentProfileId,
                Amount = 0,
                Status = PaymentStatus.Success,
                PaymentProvider = "INTERNAL",
                TxnRef = "SEEDTXN0002",
                TransactionNo = "FREE0001",
                BankCode = string.Empty,
                ResponseCode = "00",
                PayDate = now.AddDays(-30),
                RawIpnResponse = "{\"plan\":\"FreePlan\"}",
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedCreditTransactionsAsync(AppDbContext context)
    {
        if (await context.CreditTransactionLogs.AnyAsync()) return;

        var parents = await context.ParentProfiles.OrderBy(p => p.CreatedAt).ToListAsync();
        if (parents.Count == 0) return;

        context.CreditTransactionLogs.AddRange(
            new CreditTransactionLog
            {
                ParentProfileId = parents[0].Id,
                Amount = -30,
                ActionType = CreditActionType.ImageClassification,
                Description = "Classify a child's drawing.",
                CreateBy = SystemUserId
            },
            new CreditTransactionLog
            {
                ParentProfileId = parents[0].Id,
                Amount = -150,
                ActionType = CreditActionType.CharacterGeneration,
                Description = "Generate a character from drawing.",
                CreateBy = SystemUserId
            },
            new CreditTransactionLog
            {
                ParentProfileId = parents[0].Id,
                Amount = -50,
                ActionType = CreditActionType.CharacterRegeneration,
                Description = "Regenerate character preview.",
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedStoryTemplatesAsync(AppDbContext context)
    {
        if (await context.StoryTemplates.AnyAsync()) return;

        var themes = await context.EducationThemes.OrderBy(t => t.Name).ToListAsync();
        if (themes.Count < 3) return;

        context.StoryTemplates.AddRange(
            new StoryTemplate
            {
                EducationThemeId = themes[0].Id,
                Title = "The Brave Little Fox",
                Description = "A shy fox learns to be brave and help a friend.",
                AgeGroup = TargetAgeGroup.Ag6to7,
                CoverImageUrl = "/seed/covers/brave-little-fox.png",
                TotalPageNumbers = 2,
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new StoryTemplate
            {
                EducationThemeId = themes[1].Id,
                Title = "Sharing Picnic",
                Description = "Friends learn that sharing makes the picnic more fun.",
                AgeGroup = TargetAgeGroup.Age8to10,
                CoverImageUrl = "/seed/covers/sharing-picnic.png",
                TotalPageNumbers = 2,
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new StoryTemplate
            {
                EducationThemeId = themes[2].Id,
                Title = "Garden Helpers",
                Description = "Tiny garden friends take care of plants together.",
                AgeGroup = TargetAgeGroup.Age8to10,
                CoverImageUrl = "/seed/covers/garden-helpers.png",
                TotalPageNumbers = 2,
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedStoryRoleTemplatesAsync(AppDbContext context)
    {
        if (await context.StoryRoleTemplates.AnyAsync()) return;

        var stories = await context.StoryTemplates.OrderBy(s => s.Title).ToListAsync();
        if (stories.Count < 2) return;

        context.StoryRoleTemplates.AddRange(
            new StoryRoleTemplate
            {
                StoryTemplateId = stories[0].Id,
                StoryRoleType = StoryRoleType.MainCharacter,
                RoleName = "Hero",
                Description = "The main child-created hero of the story.",
                AllowCustomCharacter = true,
                RequiresParentApproval = false,
                DefaultAssetUrl = "/seed/roles/hero.png",
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new StoryRoleTemplate
            {
                StoryTemplateId = stories[0].Id,
                StoryRoleType = StoryRoleType.Companion,
                RoleName = "Best Friend",
                Description = "A loyal companion who helps the hero.",
                AllowCustomCharacter = true,
                RequiresParentApproval = true,
                DefaultAssetUrl = "/seed/roles/friend.png",
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new StoryRoleTemplate
            {
                StoryTemplateId = stories[1].Id,
                StoryRoleType = StoryRoleType.MainCharacter,
                RoleName = "Explorer",
                Description = "Leads the adventure through the story world.",
                AllowCustomCharacter = true,
                RequiresParentApproval = false,
                DefaultAssetUrl = "/seed/roles/explorer.png",
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedStoryPageTemplatesAsync(AppDbContext context)
    {
        if (await context.StoryPageTemplates.AnyAsync()) return;

        var stories = await context.StoryTemplates.OrderBy(s => s.Title).ToListAsync();
        if (stories.Count < 2) return;

        context.StoryPageTemplates.AddRange(
            new StoryPageTemplate
            {
                StoryTemplateId = stories[0].Id,
                PageNumber = 1,
                RawText = "Once upon a time, a little hero looked at a tall hill and felt a tiny bit scared.",
                BackgroundUrl = "/seed/pages/hill.png",
                AudioNarrationUrl = "/seed/audio/page-1.mp3",
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new StoryPageTemplate
            {
                StoryTemplateId = stories[0].Id,
                PageNumber = 2,
                RawText = "With a friend beside them, the hero took one small step and then another.",
                BackgroundUrl = "/seed/pages/path.png",
                AudioNarrationUrl = "/seed/audio/page-2.mp3",
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new StoryPageTemplate
            {
                StoryTemplateId = stories[1].Id,
                PageNumber = 1,
                RawText = "The explorer packed a tiny bag and set out to discover a secret garden.",
                BackgroundUrl = "/seed/pages/garden.png",
                AudioNarrationUrl = "/seed/audio/page-3.mp3",
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedCharacterSlotTemplatesAsync(AppDbContext context)
    {
        if (await context.CharacterSlotTemplates.AnyAsync()) return;

        var firstStory = await context.StoryTemplates.OrderBy(s => s.Title).FirstOrDefaultAsync();
        if (firstStory is null) return;

        var firstStoryPages = await context.StoryPageTemplates
            .Where(p => p.StoryTemplateId == firstStory.Id)
            .OrderBy(p => p.PageNumber)
            .ToListAsync();
        var firstStoryRoles = await context.StoryRoleTemplates
            .Where(r => r.StoryTemplateId == firstStory.Id)
            .OrderBy(r => r.RoleName)
            .ToListAsync();
        if (firstStoryPages.Count < 2 || firstStoryRoles.Count < 2) return;

        context.CharacterSlotTemplates.AddRange(
            new CharacterSlotTemplate
            {
                StoryPageId = firstStoryPages[0].Id,
                StoryRoleTemplateId = firstStoryRoles[0].Id,
                PosX = 120,
                PosY = 180,
                Scale = 1.00f,
                FlipHorizontal = false,
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new CharacterSlotTemplate
            {
                StoryPageId = firstStoryPages[1].Id,
                StoryRoleTemplateId = firstStoryRoles[0].Id,
                PosX = 90,
                PosY = 200,
                Scale = 1.10f,
                FlipHorizontal = true,
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new CharacterSlotTemplate
            {
                StoryPageId = firstStoryPages[1].Id,
                StoryRoleTemplateId = firstStoryRoles[1].Id,
                PosX = 260,
                PosY = 210,
                Scale = 0.95f,
                FlipHorizontal = false,
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedVocabularyTemplatesAsync(AppDbContext context)
    {
        if (await context.VocabularyTemplates.AnyAsync()) return;

        var pages = await context.StoryPageTemplates.OrderBy(p => p.PageNumber).ToListAsync();
        if (pages.Count < 2) return;

        context.VocabularyTemplates.AddRange(
            new VocabularyTemplate
            {
                StoryPageTemplateId = pages[0].Id,
                Word = "brave",
                Meaning = "Ready to try even when you feel a little scared.",
                AudioUrl = "/seed/vocab/brave.mp3",
                CreateBy = SystemUserId
            },
            new VocabularyTemplate
            {
                StoryPageTemplateId = pages[1].Id,
                Word = "friend",
                Meaning = "Someone who cares about you and helps you.",
                AudioUrl = "/seed/vocab/friend.mp3",
                CreateBy = SystemUserId
            },
            new VocabularyTemplate
            {
                StoryPageTemplateId = pages[0].Id,
                Word = "hill",
                Meaning = "A small mountain you can climb.",
                AudioUrl = "/seed/vocab/hill.mp3",
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedStoryQuizTemplatesAsync(AppDbContext context)
    {
        if (await context.StoryQuizTemplates.AnyAsync()) return;

        var stories = await context.StoryTemplates.OrderBy(s => s.Title).ToListAsync();
        if (stories.Count < 2) return;

        context.StoryQuizTemplates.AddRange(
            new StoryQuizTemplate
            {
                StoryTemplateId = stories[0].Id,
                QuestionText = "How did the hero start climbing the hill?",
                QuestionAudioUrl = "/seed/quiz/q1.mp3",
                QuestionPicUrl = "/seed/quiz/q1.png",
                OptionsJson = "[\"Took one small step\",\"Gave up\",\"Waited forever\"]",
                CorrectOptionIndex = 0,
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new StoryQuizTemplate
            {
                StoryTemplateId = stories[0].Id,
                QuestionText = "Who helped the hero feel braver?",
                QuestionAudioUrl = "/seed/quiz/q2.mp3",
                OptionsJson = "[\"A stranger\",\"A best friend\",\"Nobody\"]",
                CorrectOptionIndex = 1,
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            },
            new StoryQuizTemplate
            {
                StoryTemplateId = stories[1].Id,
                QuestionText = "What did the explorer pack for the garden trip?",
                QuestionAudioUrl = "/seed/quiz/q3.mp3",
                OptionsJson = "[\"A tiny bag\",\"A giant truck\",\"Nothing\"]",
                CorrectOptionIndex = 0,
                Status = CommonStatus.Active,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedDrawingsAsync(AppDbContext context)
    {
        if (await context.Drawings.AnyAsync()) return;

        var children = await context.ChildProfiles.OrderBy(c => c.NickName).ToListAsync();
        if (children.Count < 2) return;

        context.Drawings.AddRange(
            new Drawing
            {
                ChildProfileId = children[0].Id,
                OriginalImageUrl = "/seed/drawings/fox.png",
                DrawingType = DrawingType.Canvas,
                CreateBy = SystemUserId
            },
            new Drawing
            {
                ChildProfileId = children[0].Id,
                OriginalImageUrl = "/seed/drawings/buddy.png",
                DrawingType = DrawingType.Uploaded,
                CreateBy = SystemUserId
            },
            new Drawing
            {
                ChildProfileId = children[1].Id,
                OriginalImageUrl = "/seed/drawings/explorer.png",
                DrawingType = DrawingType.Canvas,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedAICharacterGeneratesAsync(AppDbContext context)
    {
        if (await context.AICharacterGenerates.AnyAsync()) return;

        var children = await context.ChildProfiles.OrderBy(c => c.NickName).ToListAsync();
        var drawings = await context.Drawings.OrderBy(d => d.CreatedAt).ToListAsync();
        var charTypes = await context.CharTypes.OrderBy(t => t.Name).ToListAsync();
        var child0Drawings = drawings.Where(d => d.ChildProfileId == children[0].Id).ToList();
        var child1Drawings = drawings.Where(d => d.ChildProfileId == children[1].Id).ToList();
        if (children.Count < 2 || child0Drawings.Count < 2 || child1Drawings.Count < 1 || charTypes.Count < 3) return;

        context.AICharacterGenerates.AddRange(
            new AICharacterGenerate
            {
                ChildProfileId = children[0].Id,
                DrawingId = child0Drawings[0].Id,
                CharTypeId = charTypes[0].Id,
                AIPromptUsed = "Turn this drawing into a friendly fox hero, child-safe, bright colors.",
                GenerateImageUrl = "/seed/ai/fox-hero.png",
                AIDetectedTagsJson = "[\"fox\",\"hero\",\"kind\"]",
                AIGenStatus = AIGenStatus.Success,
                CreateBy = SystemUserId
            },
            new AICharacterGenerate
            {
                ChildProfileId = children[0].Id,
                DrawingId = child0Drawings[1].Id,
                CharTypeId = charTypes[2].Id,
                AIPromptUsed = "Turn this drawing into a cheerful companion animal.",
                GenerateImageUrl = "/seed/ai/buddy.png",
                AIDetectedTagsJson = "[\"buddy\",\"animal\",\"smile\"]",
                AIGenStatus = AIGenStatus.Success,
                CreateBy = SystemUserId
            },
            new AICharacterGenerate
            {
                ChildProfileId = children[1].Id,
                DrawingId = child1Drawings[0].Id,
                CharTypeId = charTypes[1].Id,
                AIPromptUsed = "Turn this drawing into a curious child explorer.",
                GenerateImageUrl = "/seed/ai/explorer.png",
                AIDetectedTagsJson = "[\"explorer\",\"human\",\"curious\"]",
                AIGenStatus = AIGenStatus.Success,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedCharactersAsync(AppDbContext context)
    {
        if (await context.Characters.AnyAsync()) return;

        var generations = await context.AICharacterGenerates.OrderBy(g => g.GenerateImageUrl).ToListAsync();
        if (generations.Count < 3) return;

        context.Characters.AddRange(
            new Character
            {
                ChildProfileId = generations[0].ChildProfileId,
                AICharacterGenerateId = generations[0].Id,
                Name = "Foxy",
                DefaultPronoun = "they",
                FinalImageUrl = generations[0].GenerateImageUrl ?? "/seed/ai/fox-hero.png",
                Status = CharacterStatus.Approved,
                IsFavorite = true,
                CreateBy = SystemUserId
            },
            new Character
            {
                ChildProfileId = generations[1].ChildProfileId,
                AICharacterGenerateId = generations[1].Id,
                Name = "Buddy",
                DefaultPronoun = "he",
                FinalImageUrl = generations[1].GenerateImageUrl ?? "/seed/ai/buddy.png",
                Status = CharacterStatus.Approved,
                IsFavorite = false,
                CreateBy = SystemUserId
            },
            new Character
            {
                ChildProfileId = generations[2].ChildProfileId,
                AICharacterGenerateId = generations[2].Id,
                Name = "Mika",
                DefaultPronoun = "she",
                FinalImageUrl = generations[2].GenerateImageUrl ?? "/seed/ai/explorer.png",
                Status = CharacterStatus.Approved,
                IsFavorite = true,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedGeneratedStoriesAsync(AppDbContext context)
    {
        if (await context.GeneratedStories.AnyAsync()) return;

        var children = await context.ChildProfiles.OrderBy(c => c.NickName).ToListAsync();
        var stories = await context.StoryTemplates.OrderBy(s => s.Title).ToListAsync();
        if (children.Count < 2 || stories.Count < 2) return;

        context.GeneratedStories.AddRange(
            new GeneratedStory
            {
                ChildProfileId = children[0].Id,
                StoryTemplateId = stories[0].Id,
                IsFavorite = true,
                Status = GeneratedStoryStatus.Completed,
                LastPageRead = 2,
                CreateBy = SystemUserId
            },
            new GeneratedStory
            {
                ChildProfileId = children[0].Id,
                StoryTemplateId = stories[1].Id,
                IsFavorite = false,
                Status = GeneratedStoryStatus.InProgress,
                LastPageRead = 1,
                CreateBy = SystemUserId
            },
            new GeneratedStory
            {
                ChildProfileId = children[1].Id,
                StoryTemplateId = stories[1].Id,
                IsFavorite = true,
                Status = GeneratedStoryStatus.InProgress,
                LastPageRead = 1,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedUserStoryCharacterMappingsAsync(AppDbContext context)
    {
        if (await context.UserStoryCharacterMappings.AnyAsync()) return;

        var generatedStories = await context.GeneratedStories.OrderBy(s => s.CreatedAt).ToListAsync();
        var roles = await context.StoryRoleTemplates.ToListAsync();
        var characters = await context.Characters.ToListAsync();
        if (generatedStories.Count == 0) return;

        var firstStory = generatedStories[0];
        var firstStoryRoles = roles.Where(r => r.StoryTemplateId == firstStory.StoryTemplateId).OrderBy(r => r.RoleName).ToList();
        var firstChildChars = characters.Where(c => c.ChildProfileId == firstStory.ChildProfileId).OrderBy(c => c.Name).ToList();
        if (firstStoryRoles.Count == 0 || firstChildChars.Count == 0) return;

        context.UserStoryCharacterMappings.Add(
            new UserStoryCharacterMapping
            {
                GeneratedStoryId = firstStory.Id,
                StoryRoleTemplateId = firstStoryRoles[0].Id,
                CharacterId = firstChildChars[0].Id,
                ParentApprovalStatus = ParentApprovalStatus.NotRequired,
                CreateBy = SystemUserId
            });

        if (firstStoryRoles.Count > 1 && firstChildChars.Count > 1)
        {
            context.UserStoryCharacterMappings.Add(
                new UserStoryCharacterMapping
                {
                    GeneratedStoryId = firstStory.Id,
                    StoryRoleTemplateId = firstStoryRoles[1].Id,
                    CharacterId = firstChildChars[1].Id,
                    ParentApprovalStatus = ParentApprovalStatus.Approved,
                    CreateBy = SystemUserId
                });
        }

        var secondStory = generatedStories.FirstOrDefault(s => s.Id != firstStory.Id);
        if (secondStory is not null)
        {
            var secondRoles = roles.Where(r => r.StoryTemplateId == secondStory.StoryTemplateId).OrderBy(r => r.RoleName).ToList();
            var secondChars = characters.Where(c => c.ChildProfileId == secondStory.ChildProfileId).OrderBy(c => c.Name).ToList();
            if (secondRoles.Count > 0 && secondChars.Count > 0)
            {
                context.UserStoryCharacterMappings.Add(
                    new UserStoryCharacterMapping
                    {
                        GeneratedStoryId = secondStory.Id,
                        StoryRoleTemplateId = secondRoles[0].Id,
                        CharacterId = secondChars[0].Id,
                        ParentApprovalStatus = ParentApprovalStatus.NotRequired,
                        CreateBy = SystemUserId
                    });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedChildVocabularyProgressesAsync(AppDbContext context)
    {
        if (await context.ChildVocabularyProgresses.AnyAsync()) return;

        var children = await context.ChildProfiles.OrderBy(c => c.NickName).ToListAsync();
        var vocab = await context.VocabularyTemplates.OrderBy(v => v.Word).ToListAsync();
        if (children.Count == 0 || vocab.Count < 3) return;

        context.ChildVocabularyProgresses.AddRange(
            new ChildVocabularyProgress
            {
                ChildProfileId = children[0].Id,
                VocabularyTemplateId = vocab[0].Id,
                TotalListenCount = 4,
                TotalQuizAttempts = 2,
                CorrectAnswersCount = 2,
                WrongAnswersCount = 0,
                CreateBy = SystemUserId
            },
            new ChildVocabularyProgress
            {
                ChildProfileId = children[0].Id,
                VocabularyTemplateId = vocab[1].Id,
                TotalListenCount = 3,
                TotalQuizAttempts = 2,
                CorrectAnswersCount = 1,
                WrongAnswersCount = 1,
                CreateBy = SystemUserId
            },
            new ChildVocabularyProgress
            {
                ChildProfileId = children[0].Id,
                VocabularyTemplateId = vocab[2].Id,
                TotalListenCount = 1,
                TotalQuizAttempts = 1,
                CorrectAnswersCount = 0,
                WrongAnswersCount = 1,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedReadingLogsAsync(AppDbContext context)
    {
        if (await context.ReadingLogs.AnyAsync()) return;

        var generatedStories = await context.GeneratedStories.OrderBy(s => s.CreatedAt).ToListAsync();
        if (generatedStories.Count < 2) return;

        context.ReadingLogs.AddRange(
            new ReadingLog
            {
                ChildProfileId = generatedStories[0].ChildProfileId,
                GeneratedStoryId = generatedStories[0].Id,
                IsCompleted = true,
                ReadDurationSeconds = 420,
                CreateBy = SystemUserId
            },
            new ReadingLog
            {
                ChildProfileId = generatedStories[1].ChildProfileId,
                GeneratedStoryId = generatedStories[1].Id,
                IsCompleted = false,
                ReadDurationSeconds = 150,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedChildDailyUsageLogsAsync(AppDbContext context)
    {
        if (await context.ChildDailyUsageLogs.AnyAsync()) return;

        var children = await context.ChildProfiles.OrderBy(c => c.NickName).ToListAsync();
        if (children.Count < 2) return;

        var today = DateOnly.FromDateTime(SeedTime);
        context.ChildDailyUsageLogs.AddRange(
            new ChildDailyUsageLog
            {
                ChildProfileId = children[0].Id,
                LogDate = today,
                DurationSecond = 900,
                CharacterCreatedCount = 1,
                StoryReadCount = 1,
                CreateBy = SystemUserId
            },
            new ChildDailyUsageLog
            {
                ChildProfileId = children[0].Id,
                LogDate = today.AddDays(-1),
                DurationSecond = 600,
                CharacterCreatedCount = 1,
                StoryReadCount = 0,
                CreateBy = SystemUserId
            },
            new ChildDailyUsageLog
            {
                ChildProfileId = children[1].Id,
                LogDate = today,
                DurationSecond = 480,
                CharacterCreatedCount = 1,
                StoryReadCount = 1,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedChildQuizAnswersAsync(AppDbContext context)
    {
        if (await context.ChildQuizAnswers.AnyAsync()) return;

        var logs = await context.ReadingLogs.OrderBy(l => l.CreatedAt).ToListAsync();
        var quizzes = await context.StoryQuizTemplates.OrderBy(q => q.QuestionText).ToListAsync();
        if (logs.Count == 0 || quizzes.Count < 2) return;

        context.ChildQuizAnswers.AddRange(
            new ChildQuizAnswer
            {
                ChildProfileId = logs[0].ChildProfileId,
                ReadingLogId = logs[0].Id,
                StoryQuizTemplateId = quizzes[0].Id,
                QuizScore = 10,
                SelectedOptionIndex = 0,
                IsCorrect = true,
                ResponseTimeSeconds = 8,
                CreateBy = SystemUserId
            },
            new ChildQuizAnswer
            {
                ChildProfileId = logs[0].ChildProfileId,
                ReadingLogId = logs[0].Id,
                StoryQuizTemplateId = quizzes[1].Id,
                QuizScore = 0,
                SelectedOptionIndex = 2,
                IsCorrect = false,
                ResponseTimeSeconds = 12,
                CreateBy = SystemUserId
            }
        );

        if (logs.Count > 1 && quizzes.Count > 2)
        {
            context.ChildQuizAnswers.Add(
                new ChildQuizAnswer
                {
                    ChildProfileId = logs[1].ChildProfileId,
                    ReadingLogId = logs[1].Id,
                    StoryQuizTemplateId = quizzes[2].Id,
                    QuizScore = 10,
                    SelectedOptionIndex = 0,
                    IsCorrect = true,
                    ResponseTimeSeconds = 6,
                    CreateBy = SystemUserId
                });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedContentReportsAsync(AppDbContext context)
    {
        if (await context.ContentReports.AnyAsync()) return;

        var character = await context.Characters.OrderBy(c => c.Name).FirstOrDefaultAsync();
        var story = await context.GeneratedStories.OrderBy(s => s.CreatedAt).FirstOrDefaultAsync();
        if (character is null || story is null) return;

        context.ContentReports.AddRange(
            new ContentReport
            {
                TargetType = ReportTargetType.Character,
                TargetId = character.Id,
                Reason = "Character image looks confusing for young readers.",
                Status = ContentReportStatus.Pending,
                CreateBy = SystemUserId
            },
            new ContentReport
            {
                TargetType = ReportTargetType.Story,
                TargetId = story.Id,
                Reason = "Story wording may be too advanced for the age group.",
                Status = ContentReportStatus.Processing,
                CreateBy = SystemUserId
            },
            new ContentReport
            {
                TargetType = ReportTargetType.Content,
                TargetId = story.Id,
                Reason = "Background art does not match the page text.",
                Status = ContentReportStatus.Resolve,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedAIUsageLogsAsync(AppDbContext context)
    {
        if (await context.AIUsageLogs.AnyAsync()) return;

        context.AIUsageLogs.AddRange(
            new AIUsageLog
            {
                Provider = "OpenAI",
                ActionType = AIActionType.ClassifyImage,
                TokenUsed = 420,
                CostAmount = 0.012f,
                CreateBy = SystemUserId
            },
            new AIUsageLog
            {
                Provider = "OpenAI",
                ActionType = AIActionType.CharacterGenerate,
                TokenUsed = 1800,
                CostAmount = 0.085f,
                CreateBy = SystemUserId
            },
            new AIUsageLog
            {
                Provider = "Gemini",
                ActionType = AIActionType.CharacterGenerate,
                TokenUsed = 1500,
                CostAmount = 0.062f,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedAuditLogsAsync(AppDbContext context)
    {
        if (await context.AuditLogs.AnyAsync()) return;

        var child = await context.ChildProfiles.OrderBy(c => c.NickName).FirstOrDefaultAsync();
        var character = await context.Characters.OrderBy(c => c.Name).FirstOrDefaultAsync();
        var story = await context.GeneratedStories.OrderBy(s => s.CreatedAt).FirstOrDefaultAsync();
        if (child is null || character is null || story is null) return;

        context.AuditLogs.AddRange(
            new AuditLog
            {
                AuditAction = "Create",
                EntityName = nameof(ChildProfile),
                PrimaryKey = child.Id,
                OldValues = null,
                NewValues = $"{{\"NickName\":\"{child.NickName}\"}}",
                ChangedColumns = "NickName",
                CreateBy = SystemUserId
            },
            new AuditLog
            {
                AuditAction = "Update",
                EntityName = nameof(Character),
                PrimaryKey = character.Id,
                OldValues = "{\"Status\":\"Pending\"}",
                NewValues = "{\"Status\":\"Approved\"}",
                ChangedColumns = "Status",
                CreateBy = SystemUserId
            },
            new AuditLog
            {
                AuditAction = "Create",
                EntityName = nameof(GeneratedStory),
                PrimaryKey = story.Id,
                OldValues = null,
                NewValues = $"{{\"Status\":\"{story.Status}\"}}",
                ChangedColumns = "Status",
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }
}
