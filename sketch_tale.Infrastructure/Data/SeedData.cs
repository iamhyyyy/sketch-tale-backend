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
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

        try
        {
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }

            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);
            await SeedCategoriesAsync(context);
            await SeedCharTypesAsync(context);
            await SeedStoryCatalogAsync(context);
            await SeedChildDemoDataAsync(userManager, context);

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

    private static async Task SeedUsersAsync(UserManager<AppUser> userManager)
    {
        // Nếu bảng Users đã có data -> Bỏ qua không tạo user
        if (await userManager.Users.AnyAsync()) return;

        await CreateUserAsync(userManager, "admin", "admin@sketch.tale.com", "admin123", "Admin", "User", "admin");
        await CreateUserAsync(userManager, "cmanager", "cmanager@sketch.tale.com", "cmanager123", "Content", "Manager", "content manager");
        await CreateUserAsync(userManager, "parent", "parent@sketch.tale.com", "parent123", "Parent", "User", "parent");
        await CreateUserAsync(userManager, "child", "child@sketch.tale.com", "child123", "Little", "Artist", "child");
    }

    private static async Task CreateUserAsync(
        UserManager<AppUser> userManager,
        string username,
        string email,
        string password,
        string firstName,
        string lastName,
        string role)
    {
        if (await userManager.FindByNameAsync(username) != null) return;

        var user = new AppUser
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

    private static async Task SeedCategoriesAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        var now = SeedTime;
        context.Categories.AddRange(
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Adventure",
                Description = "Exciting journeys and discoveries",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Animals",
                Description = "Stories about animal friends",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Fairy Tales",
                Description = "Magical classic-style tales",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Friendship",
                Description = "Stories about kindness and friends",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedCharTypesAsync(AppDbContext context)
    {
        if (await context.CharTypes.AnyAsync()) return;

        var now = SeedTime;
        context.CharTypes.AddRange(
            new CharType
            {
                Id = Guid.NewGuid(),
                Name = "Hero",
                Description = "The main brave character",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new CharType
            {
                Id = Guid.NewGuid(),
                Name = "Sidekick",
                Description = "A helpful companion",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new CharType
            {
                Id = Guid.NewGuid(),
                Name = "Animal Friend",
                Description = "Friendly animal character",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new CharType
            {
                Id = Guid.NewGuid(),
                Name = "Magical Creature",
                Description = "Fantasy or magical being",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedStoryCatalogAsync(AppDbContext context)
    {
        if (await context.StoryTemplates.AnyAsync()) return;

        var animalsCategory = await context.Categories.FirstAsync(c => c.Name == "Animals");
        var friendshipCategory = await context.Categories.FirstAsync(c => c.Name == "Friendship");
        var now = SeedTime;

        // --- Story 1: The Brave Little Fox ---
        var storyFoxId = Guid.NewGuid();
        var foxRoleHeroId = Guid.NewGuid();
        var foxRoleFriendId = Guid.NewGuid();
        var foxPage1Id = Guid.NewGuid();
        var foxPage2Id = Guid.NewGuid();
        var foxPage3Id = Guid.NewGuid();

        context.StoryTemplates.Add(new StoryTemplate
        {
            Id = storyFoxId,
            CategoryId = animalsCategory.Id,
            Title = "The Brave Little Fox",
            Description = "A little fox learns to be brave and help a friend in the forest.",
            AgeGroup = TargetAgeGroup.Age3To5,
            CoverImageUrl = "/seed/covers/brave-little-fox.png",
            TotalPageNumbers = 3,
            Status = CommonStatus.Active,
            CreatedAt = now,
            CreateBy = SystemUserId
        });

        context.StoryRoleTemplates.AddRange(
            new StoryRoleTemplate
            {
                Id = foxRoleHeroId,
                StoryTemplateId = storyFoxId,
                RoleName = "Hero",
                Description = "The brave little fox",
                AllowCustomCharacter = true,
                RequiresParentApproval = false,
                DefaultAssetUrl = "/seed/roles/fox-hero.png",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new StoryRoleTemplate
            {
                Id = foxRoleFriendId,
                StoryTemplateId = storyFoxId,
                RoleName = "Friend",
                Description = "A forest friend who needs help",
                AllowCustomCharacter = true,
                RequiresParentApproval = false,
                DefaultAssetUrl = "/seed/roles/fox-friend.png",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            }
        );

        context.StoryPageTemplates.AddRange(
            new StoryPageTemplate
            {
                Id = foxPage1Id,
                StoryTemplateId = storyFoxId,
                PageNumber = 1,
                RawText = "One sunny morning, {Hero} walked into the green forest.",
                BackgroundUrl = "/seed/backgrounds/forest-morning.png",
                AudioNarrationUrl = "/seed/audio/fox-page1.mp3",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new StoryPageTemplate
            {
                Id = foxPage2Id,
                StoryTemplateId = storyFoxId,
                PageNumber = 2,
                RawText = "{Hero} met {Friend} near a big tree. {Friend} looked worried.",
                BackgroundUrl = "/seed/backgrounds/forest-tree.png",
                AudioNarrationUrl = "/seed/audio/fox-page2.mp3",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new StoryPageTemplate
            {
                Id = foxPage3Id,
                StoryTemplateId = storyFoxId,
                PageNumber = 3,
                RawText = "{Hero} was brave and helped {Friend} find the way home. They became best friends!",
                BackgroundUrl = "/seed/backgrounds/forest-path.png",
                AudioNarrationUrl = "/seed/audio/fox-page3.mp3",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            }
        );

        context.CharacterSlotTemplates.AddRange(
            new CharacterSlotTemplate
            {
                Id = Guid.NewGuid(),
                StoryPageId = foxPage1Id,
                StoryRoleTemplateId = foxRoleHeroId,
                PosX = 0.35f,
                PosY = 0.55f,
                Scale = 1.0f,
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new CharacterSlotTemplate
            {
                Id = Guid.NewGuid(),
                StoryPageId = foxPage2Id,
                StoryRoleTemplateId = foxRoleHeroId,
                PosX = 0.25f,
                PosY = 0.60f,
                Scale = 1.0f,
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new CharacterSlotTemplate
            {
                Id = Guid.NewGuid(),
                StoryPageId = foxPage2Id,
                StoryRoleTemplateId = foxRoleFriendId,
                PosX = 0.70f,
                PosY = 0.58f,
                Scale = 0.95f,
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new CharacterSlotTemplate
            {
                Id = Guid.NewGuid(),
                StoryPageId = foxPage3Id,
                StoryRoleTemplateId = foxRoleHeroId,
                PosX = 0.40f,
                PosY = 0.55f,
                Scale = 1.0f,
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new CharacterSlotTemplate
            {
                Id = Guid.NewGuid(),
                StoryPageId = foxPage3Id,
                StoryRoleTemplateId = foxRoleFriendId,
                PosX = 0.60f,
                PosY = 0.55f,
                Scale = 1.0f,
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            }
        );

        context.VocabularyTemplates.AddRange(
            new VocabularyTemplate
            {
                Id = Guid.NewGuid(),
                StoryPageId = foxPage3Id,
                Word = "brave",
                Meaning = "Not afraid to do something kind or hard",
                AudioUrl = "/seed/audio/vocab-brave.mp3",
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new VocabularyTemplate
            {
                Id = Guid.NewGuid(),
                StoryPageId = foxPage1Id,
                Word = "forest",
                Meaning = "A place with many trees",
                AudioUrl = "/seed/audio/vocab-forest.mp3",
                CreatedAt = now,
                CreateBy = SystemUserId
            }
        );

        context.StoryQuizTemplates.AddRange(
            new StoryQuizTemplate
            {
                Id = Guid.NewGuid(),
                StoryTemplateId = storyFoxId,
                QuestionText = "Where did the hero walk?",
                OptionsJson = "[\"In the forest\",\"In the sea\",\"In a castle\",\"On the moon\"]",
                CorrectOptionIndex = 0,
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new StoryQuizTemplate
            {
                Id = Guid.NewGuid(),
                StoryTemplateId = storyFoxId,
                QuestionText = "What did the hero do for the friend?",
                OptionsJson = "[\"Ran away\",\"Helped find the way home\",\"Ate lunch\",\"Went to sleep\"]",
                CorrectOptionIndex = 1,
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            }
        );

        // --- Story 2: Friendship Picnic ---
        var storyPicnicId = Guid.NewGuid();
        var picnicRoleHeroId = Guid.NewGuid();
        var picnicRoleFriendId = Guid.NewGuid();
        var picnicRoleGuideId = Guid.NewGuid();
        var picnicPage1Id = Guid.NewGuid();
        var picnicPage2Id = Guid.NewGuid();
        var picnicPage3Id = Guid.NewGuid();

        context.StoryTemplates.Add(new StoryTemplate
        {
            Id = storyPicnicId,
            CategoryId = friendshipCategory.Id,
            Title = "Friendship Picnic",
            Description = "Friends prepare a picnic and learn to share.",
            AgeGroup = TargetAgeGroup.Age6To8,
            CoverImageUrl = "/seed/covers/friendship-picnic.png",
            TotalPageNumbers = 3,
            Status = CommonStatus.Active,
            CreatedAt = now,
            CreateBy = SystemUserId
        });

        context.StoryRoleTemplates.AddRange(
            new StoryRoleTemplate
            {
                Id = picnicRoleHeroId,
                StoryTemplateId = storyPicnicId,
                RoleName = "Hero",
                Description = "The child who plans the picnic",
                AllowCustomCharacter = true,
                RequiresParentApproval = false,
                DefaultAssetUrl = "/seed/roles/picnic-hero.png",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new StoryRoleTemplate
            {
                Id = picnicRoleFriendId,
                StoryTemplateId = storyPicnicId,
                RoleName = "Friend",
                Description = "A friend who joins the picnic",
                AllowCustomCharacter = true,
                RequiresParentApproval = false,
                DefaultAssetUrl = "/seed/roles/picnic-friend.png",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new StoryRoleTemplate
            {
                Id = picnicRoleGuideId,
                StoryTemplateId = storyPicnicId,
                RoleName = "Guide",
                Description = "A wise helper at the park",
                AllowCustomCharacter = true,
                RequiresParentApproval = true,
                DefaultAssetUrl = "/seed/roles/picnic-guide.png",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            }
        );

        context.StoryPageTemplates.AddRange(
            new StoryPageTemplate
            {
                Id = picnicPage1Id,
                StoryTemplateId = storyPicnicId,
                PageNumber = 1,
                RawText = "{Hero} packed fruits and juice for a picnic in the park.",
                BackgroundUrl = "/seed/backgrounds/park-day.png",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new StoryPageTemplate
            {
                Id = picnicPage2Id,
                StoryTemplateId = storyPicnicId,
                PageNumber = 2,
                RawText = "{Friend} arrived with cookies. {Guide} showed them a shady spot.",
                BackgroundUrl = "/seed/backgrounds/park-trees.png",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new StoryPageTemplate
            {
                Id = picnicPage3Id,
                StoryTemplateId = storyPicnicId,
                PageNumber = 3,
                RawText = "{Hero} and {Friend} shared food and laughed together. Sharing made everyone happy!",
                BackgroundUrl = "/seed/backgrounds/park-picnic.png",
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            }
        );

        context.CharacterSlotTemplates.AddRange(
            new CharacterSlotTemplate
            {
                Id = Guid.NewGuid(),
                StoryPageId = picnicPage1Id,
                StoryRoleTemplateId = picnicRoleHeroId,
                PosX = 0.45f,
                PosY = 0.60f,
                Scale = 1.0f,
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new CharacterSlotTemplate
            {
                Id = Guid.NewGuid(),
                StoryPageId = picnicPage2Id,
                StoryRoleTemplateId = picnicRoleHeroId,
                PosX = 0.30f,
                PosY = 0.55f,
                Scale = 1.0f,
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new CharacterSlotTemplate
            {
                Id = Guid.NewGuid(),
                StoryPageId = picnicPage2Id,
                StoryRoleTemplateId = picnicRoleFriendId,
                PosX = 0.50f,
                PosY = 0.55f,
                Scale = 1.0f,
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new CharacterSlotTemplate
            {
                Id = Guid.NewGuid(),
                StoryPageId = picnicPage2Id,
                StoryRoleTemplateId = picnicRoleGuideId,
                PosX = 0.75f,
                PosY = 0.50f,
                Scale = 1.1f,
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new CharacterSlotTemplate
            {
                Id = Guid.NewGuid(),
                StoryPageId = picnicPage3Id,
                StoryRoleTemplateId = picnicRoleHeroId,
                PosX = 0.35f,
                PosY = 0.58f,
                Scale = 1.0f,
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new CharacterSlotTemplate
            {
                Id = Guid.NewGuid(),
                StoryPageId = picnicPage3Id,
                StoryRoleTemplateId = picnicRoleFriendId,
                PosX = 0.55f,
                PosY = 0.58f,
                Scale = 1.0f,
                Status = CommonStatus.Active,
                CreatedAt = now,
                CreateBy = SystemUserId
            }
        );

        context.VocabularyTemplates.AddRange(
            new VocabularyTemplate
            {
                Id = Guid.NewGuid(),
                StoryPageId = picnicPage2Id,
                Word = "friend",
                Meaning = "Someone you like and care about",
                AudioUrl = "/seed/audio/vocab-friend.mp3",
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new VocabularyTemplate
            {
                Id = Guid.NewGuid(),
                StoryPageId = picnicPage3Id,
                Word = "share",
                Meaning = "To give some of what you have to others",
                AudioUrl = "/seed/audio/vocab-share.mp3",
                CreatedAt = now,
                CreateBy = SystemUserId
            }
        );

        context.StoryQuizTemplates.Add(new StoryQuizTemplate
        {
            Id = Guid.NewGuid(),
            StoryTemplateId = storyPicnicId,
            QuestionText = "What made everyone happy at the end?",
            OptionsJson = "[\"Hiding food\",\"Sharing food\",\"Leaving the park\",\"Sleeping\"]",
            CorrectOptionIndex = 1,
            Status = CommonStatus.Active,
            CreatedAt = now,
            CreateBy = SystemUserId
        });

        await context.SaveChangesAsync();
    }

    private static async Task SeedChildDemoDataAsync(UserManager<AppUser> userManager, AppDbContext context)
    {
        if (await context.ChildProfiles.AnyAsync()) return;

        var parent = await userManager.FindByNameAsync("parent");
        var childUser = await userManager.FindByNameAsync("child");
        if (parent is null || childUser is null)
        {
            Console.WriteLine("Skip child demo seed: parent or child user not found.");
            return;
        }

        var animalsCategory = await context.Categories.FirstAsync(c => c.Name == "Animals");
        var friendshipCategory = await context.Categories.FirstAsync(c => c.Name == "Friendship");
        var heroType = await context.CharTypes.FirstAsync(c => c.Name == "Hero");
        var animalType = await context.CharTypes.FirstAsync(c => c.Name == "Animal Friend");
        var foxStory = await context.StoryTemplates.FirstAsync(s => s.Title == "The Brave Little Fox");
        var foxRoles = await context.StoryRoleTemplates.Where(r => r.StoryTemplateId == foxStory.Id).ToListAsync();
        var foxRoleHero = foxRoles.First(r => r.RoleName == "Hero");
        var foxRoleFriend = foxRoles.First(r => r.RoleName == "Friend");
        var foxQuizzes = await context.StoryQuizTemplates.Where(q => q.StoryTemplateId == foxStory.Id).OrderBy(q => q.QuestionText).ToListAsync();
        var vocabBrave = await context.VocabularyTemplates.FirstAsync(v => v.Word == "brave");
        var vocabForest = await context.VocabularyTemplates.FirstAsync(v => v.Word == "forest");

        var now = SeedTime;
        var childProfileId = Guid.NewGuid();
        var drawing1Id = Guid.NewGuid();
        var drawing2Id = Guid.NewGuid();
        var characterFoxId = Guid.NewGuid();
        var characterBunnyId = Guid.NewGuid();
        var generatedStoryId = Guid.NewGuid();
        var readingLogId = Guid.NewGuid();

        context.ChildProfiles.Add(new ChildProfile
        {
            Id = childProfileId,
            UserId = childUser.Id,
            ParentIdM = parent.Id,
            NickName = "Mimi",
            TargetAgeGroup = TargetAgeGroup.Age3To5,
            DailyTimeLimit = 30,
            DailyCharacterLimit = 5,
            AllowedCategoryIdsJson = $"[\"{animalsCategory.Id}\",\"{friendshipCategory.Id}\"]",
            CreatedAt = now,
            CreateBy = SystemUserId
        });

        context.Drawings.AddRange(
            new Drawing
            {
                Id = drawing1Id,
                ChildId = childProfileId,
                OriginalImageUrl = "/seed/drawings/mimi-fox.png",
                DrawingType = DrawingType.Canvas,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new Drawing
            {
                Id = drawing2Id,
                ChildId = childProfileId,
                OriginalImageUrl = "/seed/drawings/mimi-bunny.png",
                DrawingType = DrawingType.Uploaded,
                CreatedAt = now,
                CreateBy = SystemUserId
            }
        );

        context.Characters.AddRange(
            new Character
            {
                Id = characterFoxId,
                ChildId = childProfileId,
                DrawingId = drawing1Id,
                CharTypeId = heroType.Id,
                Name = "Foxy",
                DefaultPronoun = "he/him",
                ProcessedSpriteUrl = "/seed/sprites/foxy.png",
                AIDetectedTagsJson = "[\"fox\",\"orange\",\"cute\"]",
                Status = CommonStatus.Active,
                ParentApprovalStatus = ParentApprovalStatus.Approved,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new Character
            {
                Id = characterBunnyId,
                ChildId = childProfileId,
                DrawingId = drawing2Id,
                CharTypeId = animalType.Id,
                Name = "BunBun",
                DefaultPronoun = "she/her",
                ProcessedSpriteUrl = "/seed/sprites/bunbun.png",
                AIDetectedTagsJson = "[\"bunny\",\"soft\",\"friend\"]",
                Status = CommonStatus.Active,
                ParentApprovalStatus = ParentApprovalStatus.Approved,
                CreatedAt = now,
                CreateBy = SystemUserId
            }
        );

        context.GeneratedStories.Add(new GeneratedStory
        {
            Id = generatedStoryId,
            ChildId = childProfileId,
            StoryTemplateId = foxStory.Id,
            IsFavorite = true,
            Status = CommonStatus.Active,
            LastPageRead = 3,
            CreatedAt = now,
            CreateBy = SystemUserId
        });

        context.UserStoryCharacterMappings.AddRange(
            new UserStoryCharacterMapping
            {
                Id = Guid.NewGuid(),
                GeneratedStoryId = generatedStoryId,
                StoryRoleTemplateId = foxRoleHero.Id,
                CharacterId = characterFoxId,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new UserStoryCharacterMapping
            {
                Id = Guid.NewGuid(),
                GeneratedStoryId = generatedStoryId,
                StoryRoleTemplateId = foxRoleFriend.Id,
                CharacterId = characterBunnyId,
                CreatedAt = now,
                CreateBy = SystemUserId
            }
        );

        context.ReadingLogs.Add(new ReadingLog
        {
            Id = readingLogId,
            ChildId = childProfileId,
            GeneratedStoryId = generatedStoryId,
            IsCompleted = true,
            ReadDurationSeconds = 420,
            ReadAt = now,
            CreatedAt = now,
            CreateBy = SystemUserId
        });

        context.ChildQuizAnswers.AddRange(
            new ChildQuizAnswer
            {
                Id = Guid.NewGuid(),
                ChildId = childProfileId,
                ReadingLogId = readingLogId,
                StoryQuizTemplateId = foxQuizzes[0].Id,
                QuizScore = 10,
                SelectedOptionIndex = foxQuizzes[0].CorrectOptionIndex,
                IsCorrect = true,
                ResponseTimeSeconds = 8,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new ChildQuizAnswer
            {
                Id = Guid.NewGuid(),
                ChildId = childProfileId,
                ReadingLogId = readingLogId,
                StoryQuizTemplateId = foxQuizzes[1].Id,
                QuizScore = 10,
                SelectedOptionIndex = foxQuizzes[1].CorrectOptionIndex,
                IsCorrect = true,
                ResponseTimeSeconds = 12,
                CreatedAt = now,
                CreateBy = SystemUserId
            }
        );

        context.ChildVocabularyProgresses.AddRange(
            new ChildVocabularyProgress
            {
                Id = Guid.NewGuid(),
                ChildId = childProfileId,
                VocabularyId = vocabBrave.Id,
                TotalListenCount = 3,
                TotalQuizAttempts = 1,
                CorrectAnswersCount = 1,
                WrongAnswersCount = 0,
                CreatedAt = now,
                CreateBy = SystemUserId
            },
            new ChildVocabularyProgress
            {
                Id = Guid.NewGuid(),
                ChildId = childProfileId,
                VocabularyId = vocabForest.Id,
                TotalListenCount = 2,
                TotalQuizAttempts = 1,
                CorrectAnswersCount = 1,
                WrongAnswersCount = 0,
                CreatedAt = now,
                CreateBy = SystemUserId
            }
        );

        await context.SaveChangesAsync();
    }
}
