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
            //await SeedParentProfilesAsync(context, userManager);
            //await SeedContentManagerProfilesAsync(context, userManager);
            //await SeedParentProfileSubsAsync(context);
            //await SeedChildProfilesAsync(context, userManager);
            //await SeedSubscriptionsAsync(context);
            //await SeedPaymentTransactionsAsync(context);
            //await SeedCreditTransactionsAsync(context);
            //await SeedStoryTemplatesAsync(context);
            //await SeedStoryRoleTemplatesAsync(context);
            //await SeedStoryPageTemplatesAsync(context);
            //await SeedCharacterSlotTemplatesAsync(context);
            //await SeedVocabularyTemplatesAsync(context);
            //await SeedStoryQuizTemplatesAsync(context);
            //await SeedDrawingsAsync(context);
            //await SeedAICharacterGeneratesAsync(context);
            //await SeedCharactersAsync(context);
            //await SeedGeneratedStoriesAsync(context);
            //await SeedUserStoryCharacterMappingsAsync(context);
            //await SeedChildVocabularyProgressesAsync(context);
            //await SeedReadingLogsAsync(context);
            //await SeedChildDailyUsageLogsAsync(context);
            //await SeedChildQuizAnswersAsync(context);
            //await SeedContentReportsAsync(context);
            //await SeedAIUsageLogsAsync(context);

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

        context.SubscriptionPlans.AddRange(
            new SubscriptionPlan
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),

                Name = "Free", Price = 0, Code = "FREEPLAN",

                ChildProfileLimit = 1, MonthlyCreditLimit = 500,

                DurationDays = 30, CreateBy = SystemUserId
            },
            new SubscriptionPlan
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),

                Name = "Plus", Price = 49000, Code = "PLUSPLAN",

                ChildProfileLimit = 3, MonthlyCreditLimit = 4000, 

                AccessFullStories = true, CanExportStory = true, HaveParentProfileSub = true,

                DurationDays = 30, FreeTrialDays = 7, CreateBy = SystemUserId
            },
            new SubscriptionPlan
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),

                Name = "Pro", Price = 99000, Code = "PROPLAN",

                ChildProfileLimit = 5, MonthlyCreditLimit = 10000,

                AccessFullStories = true, CanExportStory = true, HaveParentProfileSub = true,

                DurationDays = 30, CreateBy = SystemUserId
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
                CreateBy = SystemUserId
            },
            new CreditCost
            {
                FeatureKey = "CharacterGeneration",
                Cost = 150,
                FeatureNameVi = "Tạo nhân vật",
                FeatureNameEn = "Character Generation",
                CreateBy = SystemUserId
            },
            new CreditCost
            {
                FeatureKey = "CharacterRegeneration",
                Cost = 50,
                FeatureNameVi = "Tạo lại nhân vật",
                FeatureNameEn = "Character Regeneration",
                CreateBy = SystemUserId
            },
            new CreditCost
            {
                FeatureKey = "AssignCharacter",
                Cost = 20,
                FeatureNameVi = "Gán nhân vật vô truyện",
                FeatureNameEn = "Assign Character",
                CreateBy = SystemUserId
            },
            new CreditCost
            {
                FeatureKey = "ExportStory",
                Cost = 200,
                FeatureNameVi = "Xuất câu truyện",
                FeatureNameEn = "Export Story",
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

        var charTypeItems = new (string NameVi, string NameEn, CharTypeCategory Category, bool CanFly, bool CanSwim)[]
        {
            ("máy bay", "airplane", CharTypeCategory.Vehicle, true, false),
            ("thiên thần", "angel", CharTypeCategory.Fantasy, true, false),
            ("kiến", "ant", CharTypeCategory.Animal, false, false),
            ("dơi", "bat", CharTypeCategory.Animal, true, false),
            ("gấu", "bear", CharTypeCategory.Animal, false, true),
            ("ong", "bee", CharTypeCategory.Animal, true, false),
            ("chim", "bird", CharTypeCategory.Animal, true, false),
            ("xe buýt", "bus", CharTypeCategory.Vehicle, false, false),
            ("bướm", "butterfly", CharTypeCategory.Animal, true, false),
            ("xương rồng", "cactus", CharTypeCategory.NatureAndObject, false, false),
            ("lạc đà", "camel", CharTypeCategory.Animal, false, false),
            ("nến", "candle", CharTypeCategory.NatureAndObject, false, false),
            ("ô tô", "car", CharTypeCategory.Vehicle, false, false),
            ("cà rốt", "carrot", CharTypeCategory.NatureAndObject, false, false),
            ("lâu đài", "castle", CharTypeCategory.NatureAndObject, false, false),
            ("mèo", "cat", CharTypeCategory.Animal, false, false),
            ("bò", "cow", CharTypeCategory.Animal, false, false),
            ("cua", "crab", CharTypeCategory.Animal, false, true),
            ("cá sấu", "crocodile", CharTypeCategory.Animal, false, true),
            ("chó", "dog", CharTypeCategory.Animal, false, true),
            ("cá heo", "dolphin", CharTypeCategory.Animal, false, true),
            ("rồng", "dragon", CharTypeCategory.Fantasy, true, false),
            ("vịt", "duck", CharTypeCategory.Animal, true, true),
            ("voi", "elephant", CharTypeCategory.Animal, false, true),
            ("cá", "fish", CharTypeCategory.Animal, false, true),
            ("chim hồng hạc", "flamingo", CharTypeCategory.Animal, true, true),
            ("hoa", "flower", CharTypeCategory.NatureAndObject, false, false),
            ("ếch", "frog", CharTypeCategory.Animal, false, true),
            ("hươu cao cổ", "giraffe", CharTypeCategory.Animal, false, false),
            ("dê", "goat", CharTypeCategory.Animal, false, false),
            ("nhím", "hedgehog", CharTypeCategory.Animal, false, false),
            ("ngựa", "horse", CharTypeCategory.Animal, false, false),
            ("chuột túi", "kangaroo", CharTypeCategory.Animal, false, false),
            ("sư tử", "lion", CharTypeCategory.Animal, false, false),
            ("tôm hùm", "lobster", CharTypeCategory.Animal, false, true),
            ("nàng tiên cá", "mermaid", CharTypeCategory.Fantasy, false, true),
            ("khỉ", "monkey", CharTypeCategory.Animal, false, false),
            ("mặt trăng", "moon", CharTypeCategory.NatureAndObject, false, false),
            ("muỗi", "mosquito", CharTypeCategory.Animal, true, false),
            ("chuột", "mouse", CharTypeCategory.Animal, false, false),
            ("nấm", "mushroom", CharTypeCategory.NatureAndObject, false, false),
            ("bạch tuộc", "octopus", CharTypeCategory.Animal, false, true),
            ("hành tây", "onion", CharTypeCategory.NatureAndObject, false, false),
            ("cú", "owl", CharTypeCategory.Animal, true, false),
            ("gấu trúc", "panda", CharTypeCategory.Animal, false, false),
            ("vẹt", "parrot", CharTypeCategory.Animal, true, false),
            ("đậu phộng", "peanut", CharTypeCategory.NatureAndObject, false, false),
            ("quả lê", "pear", CharTypeCategory.NatureAndObject, false, false),
            ("đậu hà lan", "peas", CharTypeCategory.NatureAndObject, false, false),
            ("bút chì", "pencil", CharTypeCategory.NatureAndObject, false, false),
            ("chim cánh cụt", "penguin", CharTypeCategory.Animal, false, true),
            ("lợn", "pig", CharTypeCategory.Animal, false, false),
            ("dứa", "pineapple", CharTypeCategory.NatureAndObject, false, false),
            ("khoai tây", "potato", CharTypeCategory.NatureAndObject, false, false),
            ("thỏ", "rabbit", CharTypeCategory.Animal, false, false),
            ("gấu mèo", "raccoon", CharTypeCategory.Animal, false, false),
            ("cầu vồng", "rainbow", CharTypeCategory.NatureAndObject, false, false),
            ("tê giác", "rhinoceros", CharTypeCategory.Animal, false, false),
            ("thuyền buồm", "sailboat", CharTypeCategory.Vehicle, false, true),
            ("bọ cạp", "scorpion", CharTypeCategory.Animal, false, false),
            ("rùa biển", "sea turtle", CharTypeCategory.Animal, false, true),
            ("cá mập", "shark", CharTypeCategory.Animal, false, true),
            ("cừu", "sheep", CharTypeCategory.Animal, false, false),
            ("tòa nhà chọc trời", "skyscraper", CharTypeCategory.NatureAndObject, false, false),
            ("ốc sên", "snail", CharTypeCategory.Animal, false, false),
            ("rắn", "snake", CharTypeCategory.Animal, false, true),
            ("bông tuyết", "snowflake", CharTypeCategory.NatureAndObject, false, false),
            ("người tuyết", "snowman", CharTypeCategory.Fantasy, false, false),
            ("nhện", "spider", CharTypeCategory.Animal, false, false),
            ("nét vẽ nguệch ngoạc", "scribble", CharTypeCategory.NatureAndObject, false, false),
            ("sóc", "squirrel", CharTypeCategory.Animal, false, false),
            ("ngôi sao", "star", CharTypeCategory.NatureAndObject, false, false),
            ("dâu tây", "strawberry", CharTypeCategory.NatureAndObject, false, false),
            ("mặt trời", "sun", CharTypeCategory.NatureAndObject, false, false),
            ("thiên nga", "swan", CharTypeCategory.Animal, true, true),
            ("gấu bông", "teddy bear", CharTypeCategory.NatureAndObject, false, false),
            ("hổ", "tiger", CharTypeCategory.Animal, false, true),
            ("tàu hỏa", "train", CharTypeCategory.Vehicle, false, false),
            ("cây", "tree", CharTypeCategory.NatureAndObject, false, false),
            ("xe tải", "truck", CharTypeCategory.Vehicle, false, false),
            ("ô", "umbrella", CharTypeCategory.NatureAndObject, false, false),
            ("bình hoa", "vase", CharTypeCategory.NatureAndObject, false, false),
            ("dưa hấu", "watermelon", CharTypeCategory.NatureAndObject, false, false),
            ("cá voi", "whale", CharTypeCategory.Animal, false, true),
            ("ngựa vằn", "zebra", CharTypeCategory.Animal, false, false)
        };

        var charTypes = charTypeItems.Select(item => new CharType
        {
            NameVi = item.NameVi,
            NameEn = item.NameEn,
            Category = item.Category,
            CanFly = item.CanFly,
            CanSwim = item.CanSwim,
            Status = CommonStatus.Active,
            ImageUrl = null,
            CreateBy = SystemUserId
        });

        await context.CharTypes.AddRangeAsync(charTypes);
        await context.SaveChangesAsync();
    }

    private static async Task SeedEducationThemesAsync(AppDbContext context)
    {
        if (await context.EducationThemes.AnyAsync()) return;

        var themes = new (string NameVi, string NameEn, string Desc)[]
        {
            ("Quản lý cảm xúc", "Emotional management", "Nhận diện và xử lý các cảm xúc tiêu cực như tức giận, sợ hãi, buồn rầu, ghen tị."),
            ("Tự tin", "Self-confidence", "Tin tưởng vào năng lực của bản thân, tự hào về điểm riêng biệt của mình, không rụt rè."),
            ("Lòng dũng cảm", "Courage", "Dũng cảm đối mặt với bóng tối, thử thách mới, dám nhận lỗi và sửa sai."),
            ("Tính kiên trì", "Perseverance", "Học cách vượt qua thất bại, kiên trì luyện tập để đạt mục tiêu."),
            ("Tính tự lập", "Independence", "Tự dọn dẹp đồ chơi, tự vệ sinh cá nhân, chịu trách nhiệm về hành động của mình."),
            ("Lòng tốt", "Kindness", "Biết nhường nhịn đồ chơi, giúp đỡ người khác, lan tỏa sự ấm áp."),
            ("Lòng trung thực", "Honesty", "Không nói dối, không gian dối, trả lại đồ nhặt được, dũng cảm nhận lỗi."),
            ("Lòng biết ơn", "Gratitude", "Biết ơn cha mẹ, thầy cô, thức ăn hàng ngày và những điều nhỏ bé xung quanh."),
            ("Tôn trọng sự khác biệt", "Respecting differences", "Không phân biệt đối xử với bạn bè có ngoại hình, hoàn cảnh hoặc tính cách khác biệt."),
            ("Khiêm tốn", "humility", "Không kiêu ngạo khi chiến thắng, biết lắng nghe và học hỏi từ người khác."),
            ("Tình bạn", "Friendship", "Học cách kết bạn, hợp tác giải quyết vấn đề, tôn trọng ý kiến đồng đội."),
            ("An toàn cá nhân", "Personal safety", "Cảnh giác với người lạ, an toàn giao thông, an toàn khi ở nhà, quy tắc bảo vệ cơ thể."),
            ("Thói quen lành mạnh", "healthy habits", "Ăn nhiều rau củ, đi ngủ đúng giờ, tập thể dục, hạn chế xem màn hình điện tử."),
            ("Quản lý tài chính nhỏ", "Basic financial literacy", "Hiểu giá trị của lao động, thói quen tiết kiệm tiền heo đất, phân biệt 'muốn' và 'cần'."),
            ("Ứng xử", "Good manners", "Nói lời 'cảm ơn', 'xin lỗi', thưa gửi người lớn, không ngắt lời người khác."),
            ("Bảo vệ môi trường", "Environmental protection", "Phân loại rác, tiết kiệm nước/điện, yêu thương và bảo vệ động vật, trồng cây xanh."),
            ("Tư duy sáng tạo", "Creative thinking", "Khuyến khích đặt câu hỏi 'Tại sao?', tìm nhiều cách giải quyết cho một bài toán/tình huống."),
            ("Khoa học & Vũ trụ", "Science & Space", "Sự tò mò về thế giới xung quanh, các hiện tượng tự nhiên (mưa, nắng, cầu vồng, các hành tinh)."),
            ("Tình yêu gia đình", "Family love", "Vấn đề hòa thuận giữa anh chị em trong nhà, tình cảm ông bà, cha mẹ.")
        };

        var seedEntities = themes.Select(t => new EducationTheme
        {
            NameVi = t.NameVi,
            NameEn = t.NameEn,
            Description = t.Desc,
            Status = CommonStatus.Active,
            CreateBy = SystemUserId
        });

        await context.EducationThemes.AddRangeAsync(seedEntities);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRestrictedKeywordsAsync(AppDbContext context)
    {
        if (await context.RestrictedKeywords.AnyAsync()) return;

        var keywords = new[]
        {
            // Bạo lực & Giết chóc
            new { Kw = "giết", Desc = "Từ ngữ bạo lực gây sợ hãi cho trẻ", Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.BlockImmediately, Act = ModerationAction.Reject },
            new { Kw = "máu me", Desc = "Miêu tả hình ảnh bạo lực", Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.High, Act = ModerationAction.Reject },
            new { Kw = "tự tử", Desc = "Hành vi tự hại", Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.BlockImmediately, Act = ModerationAction.Reject },
            new { Kw = "tự sát", Desc = "Hành vi tự hại", Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.BlockImmediately, Act = ModerationAction.Reject },
            new { Kw = "chém giết", Desc = "Hành vi bạo lực", Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.High, Act = ModerationAction.Reject },

            // Chất cấm & Tệ nạn
            new { Kw = "ma túy", Desc = "Chất gây nghiện bất hợp pháp", Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.High, Act = ModerationAction.Reject },
            new { Kw = "thuốc lá", Desc = "Chất kích thích", Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.Medium, Act = ModerationAction.FlagForReview },
            new { Kw = "rượu bia", Desc = "Đồ uống có cồn", Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.Low, Act = ModerationAction.FlagForReview },
            new { Kw = "cờ bạc", Desc = "Hành vi tệ nạn xã hội", Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.High, Act = ModerationAction.Reject },

            // Tình dục & Nhạy cảm
            new { Kw = "sex", Desc = "Nội dung người lớn", Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.BlockImmediately, Act = ModerationAction.Reject },
            new { Kw = "khiêu dâm", Desc = "Nội dung người lớn", Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.BlockImmediately, Act = ModerationAction.Reject },
            new { Kw = "khêu gợi", Desc = "Nội dung không phù hợp lứa tuổi", Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.High, Act = ModerationAction.Reject },

            // Thông tin cá nhân (Bảo vệ an toàn cho trẻ)
            new { Kw = "số điện thoại", Desc = "Thu thập thông tin cá nhân", Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.Medium, Act = ModerationAction.Mask },
            new { Kw = "mật khẩu", Desc = "Thông tin nhạy cảm", Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.High, Act = ModerationAction.Mask },
            new { Kw = "địa chỉ nhà", Desc = "Thu thập thông tin cá nhân", Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.Medium, Act = ModerationAction.FlagForReview }
        };

        var seedEntities = keywords.Select(k => new RestrictedKeyword
        {
            Keyword = k.Kw,
            Description = k.Desc,
            MatchType = k.Match,
            SeverityLevel = k.Severity,
            Action = k.Act,
            Status = CommonStatus.Active,
            CreateBy = SystemUserId
        });

        await context.RestrictedKeywords.AddRangeAsync(seedEntities);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProhibitedThemesAsync(AppDbContext context)
    {
        if (await context.ProhibitedThemes.AnyAsync()) return;

        var themes = new[]
        {
            new
            {
                Name = "Kinh dị & Ma quái quá đà",
                Desc = "Các câu chuyện chứa yếu tố hù dọa mạnh, ma quỷ máu me, quái vật ghê rợn gây ám ảnh tâm lý cho trẻ.",
                Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.High, Act = ModerationAction.Reject
            },
            new
            {
                Name = "Bạo lực & Xung đột vũ trang",
                Desc = "Cốt truyện cổ xúy đánh nhau, chiến tranh tàn khốc, sử dụng vũ khí nguy hiểm (súng, dao, chất nổ).",
                Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.High, Act = ModerationAction.Reject
            },
            new
            {
                Name = "Nội dung người lớn & Nhạy cảm",
                Desc = "Các chủ đề liên quan đến tình dục, mối quan hệ yêu đương không phù hợp lứa tuổi thiếu nhi.",
                Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.BlockImmediately, Act = ModerationAction.Reject
            },
            new
            {
                Name = "Hành vi tự hại & Tự tử",
                Desc = "Các chủ đề đề cập, khuyến khích hoặc miêu tả hành vi tự làm tổn thương bản thân.",
                Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.BlockImmediately, Act = ModerationAction.Reject
            },
            new
            {
                Name = "Bắt nạt & Kỳ thị xã hội",
                Desc = "Chủ đề dung dưỡng cho hành vi miệt thị ngoại hình, phân biệt chủng tộc, giới tính hoặc cô lập bạn bè.",
                Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.Medium, Act = ModerationAction.FlagForReview
            },
            new
            {
                Name = "Sử dụng chất kích thích & Tệ nạn",
                Desc = "Miêu tả việc sử dụng thuốc lá, rượu bia, ma túy hoặc tham gia cá cược, cờ bạc.",
                Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.High, Act = ModerationAction.Reject
            },
            new
            {
                Name = "Mê tín dị đoan & Cực đoan",
                Desc = "Tuyên truyền các hủ tục độc hại, tà giáo hoặc tư tưởng mê tín gây lệch lạc nhận thức.",
                Match = Domain.Enums.MatchType.Contains, Severity = SeverityLevel.Medium, Act = ModerationAction.FlagForReview
            }
        };

        var seedEntities = themes.Select(t => new ProhibitedTheme
        {
            Name = t.Name,
            Description = t.Desc,
            MatchType = t.Match,
            SeverityLevel = t.Severity,
            Action = t.Act,
            Status = CommonStatus.Active,
            CreateBy = SystemUserId
        });

        await context.ProhibitedThemes.AddRangeAsync(seedEntities);
        await context.SaveChangesAsync();
    }

    //private static async Task SeedParentProfilesAsync(AppDbContext context, UserManager<User> userManager)
    //{
    //    if (await context.ParentProfiles.AnyAsync()) return;

    //    var parent = await userManager.FindByNameAsync("parent");
    //    var parent2 = await userManager.FindByNameAsync("parent2");
    //    if (parent is null || parent2 is null) return;

    //    context.ParentProfiles.AddRange(
    //        new ParentProfile
    //        {
    //            Id = parent.Id,
    //            RemainingMonthlyCredit = 3720,
    //            TrialPlusPlan = true,
    //            CreateBy = SystemUserId
    //        },
    //        new ParentProfile
    //        {
    //            Id = parent2.Id,
    //            RemainingMonthlyCredit = 470,
    //            TrialPlusPlan = false,
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedContentManagerProfilesAsync(AppDbContext context, UserManager<User> userManager)
    //{
    //    if (await context.ContentManagerProfiles.AnyAsync()) return;

    //    var manager = await userManager.FindByNameAsync("manager");
    //    if (manager is null) return;

    //    context.ContentManagerProfiles.Add(
    //        new ContentManagerProfile
    //        {
    //            Id = manager.Id,
    //            UploadImageUrl = new List<string>(),
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedParentProfileSubsAsync(AppDbContext context)
    //{
    //    if (await context.ParentProfileSubs.AnyAsync()) return;

    //    var parents = await context.ParentProfiles.OrderBy(p => p.CreatedAt).ToListAsync();
    //    if (parents.Count < 2) return;

    //    context.ParentProfileSubs.AddRange(
    //        new ParentProfileSub
    //        {
    //            Id = parents[0].Id,
    //            ParentProfileId = parents[0].Id,
    //            ParentProfile = parents[0],
    //            CreateBy = SystemUserId
    //        },
    //        new ParentProfileSub
    //        {
    //            Id = parents[1].Id,
    //            ParentProfileId = parents[1].Id,
    //            ParentProfile = parents[1],
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedChildProfilesAsync(AppDbContext context, UserManager<User> userManager)
    //{
    //    if (await context.ChildProfiles.AnyAsync()) return;

    //    var child = await userManager.FindByNameAsync("child");
    //    var child2 = await userManager.FindByNameAsync("child2");
    //    var parents = await context.ParentProfiles.OrderBy(p => p.CreatedAt).ToListAsync();
    //    var themes = await context.EducationThemes.OrderBy(t => t.NameVi).ToListAsync();
    //    if (child is null || child2 is null || parents.Count < 2) return;

    //    var allowedThemes = themes.Count >= 2
    //        ? $"[{string.Join(",", themes.Take(2).Select(t => $"\"{t.Id}\""))}]"
    //        : null;

    //    context.ChildProfiles.AddRange(
    //        new ChildProfile
    //        {
    //            Id = child.Id,
    //            ParentProfileId = parents[0].Id,
    //            NickName = "Little Artist",
    //            TargetAgeGroup = TargetAgeGroup.Ag6to7,
    //            ReadingLevel = ReadingLevel.Beginner,
    //            DailyTimeLimit = 30,
    //            DailyCharacterLimit = 2,
    //            RemainingCharacterLimit = 1,
    //            CreateBy = SystemUserId
    //        },
    //        new ChildProfile
    //        {
    //            Id = child2.Id,
    //            ParentProfileId = parents[1].Id,
    //            NickName = "Little Dreamer",
    //            TargetAgeGroup = TargetAgeGroup.Age8to10,
    //            ReadingLevel = ReadingLevel.Intermediate,
    //            DailyTimeLimit = 45,
    //            DailyCharacterLimit = 1,
    //            RemainingCharacterLimit = 0,
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedSubscriptionsAsync(AppDbContext context)
    //{
    //    if (await context.Subscriptions.AnyAsync()) return;

    //    var parents = await context.ParentProfiles.OrderBy(p => p.CreatedAt).ToListAsync();
    //    var plusPlan = await context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Code == "PlusPlan");
    //    var freePlan = await context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Code == "FreePlan");
    //    if (parents.Count < 2 || plusPlan is null || freePlan is null) return;

    //    var now = SeedTime;
    //    context.Subscriptions.AddRange(
    //        new Subscription
    //        {
    //            ParentProfileId = parents[0].Id,
    //            SubscriptionPlanId = plusPlan.Id,
    //            Status = SubscriptionStatus.Active,
    //            StartDate = now.AddDays(-10),
    //            EndDate = now.AddDays(20),
    //            CreateBy = SystemUserId
    //        },
    //        new Subscription
    //        {
    //            ParentProfileId = parents[1].Id,
    //            SubscriptionPlanId = freePlan.Id,
    //            Status = SubscriptionStatus.Active,
    //            StartDate = now.AddDays(-30),
    //            EndDate = now.AddDays(9969),
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedPaymentTransactionsAsync(AppDbContext context)
    //{
    //    if (await context.PaymentTransactions.AnyAsync()) return;

    //    var subscriptions = await context.Subscriptions.OrderBy(s => s.CreatedAt).ToListAsync();
    //    if (subscriptions.Count < 2) return;

    //    var now = SeedTime;
    //    context.PaymentTransactions.AddRange(
    //        new PaymentTransaction
    //        {
    //            SubscriptionId = subscriptions[0].Id,
    //            ParentProfileId = subscriptions[0].ParentProfileId,
    //            Amount = 49,
    //            Status = PaymentStatus.Success,
    //            PaymentProvider = "VNPAY",
    //            TxnRef = "SEEDTXN0001",
    //            TransactionNo = "13800123",
    //            BankCode = "NCB",
    //            ResponseCode = "00",
    //            PayDate = now.AddDays(-10),
    //            RawIpnResponse = "{\"vnp_ResponseCode\":\"00\"}",
    //            CreateBy = SystemUserId
    //        },
    //        new PaymentTransaction
    //        {
    //            SubscriptionId = subscriptions[1].Id,
    //            ParentProfileId = subscriptions[1].ParentProfileId,
    //            Amount = 0,
    //            Status = PaymentStatus.Success,
    //            PaymentProvider = "INTERNAL",
    //            TxnRef = "SEEDTXN0002",
    //            TransactionNo = "FREE0001",
    //            BankCode = string.Empty,
    //            ResponseCode = "00",
    //            PayDate = now.AddDays(-30),
    //            RawIpnResponse = "{\"plan\":\"FreePlan\"}",
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedCreditTransactionsAsync(AppDbContext context)
    //{
    //    if (await context.CreditTransactionLogs.AnyAsync()) return;

    //    var parents = await context.ParentProfiles.OrderBy(p => p.CreatedAt).ToListAsync();
    //    if (parents.Count == 0) return;

    //    context.CreditTransactionLogs.AddRange(
    //        new CreditTransactionLog
    //        {
    //            ParentProfileId = parents[0].Id,
    //            Amount = 30,
    //            ActionType = "ImageClassification",
    //            CreateBy = SystemUserId
    //        },
    //        new CreditTransactionLog
    //        {
    //            ParentProfileId = parents[0].Id,
    //            Amount = 150,
    //            ActionType = "CharacterGeneration",
    //            CreateBy = SystemUserId
    //        },
    //        new CreditTransactionLog
    //        {
    //            ParentProfileId = parents[0].Id,
    //            Amount = 50,
    //            ActionType = "CharacterRegeneration",
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedStoryTemplatesAsync(AppDbContext context)
    //{
    //    if (await context.StoryTemplates.AnyAsync()) return;

    //    var themes = await context.EducationThemes.OrderBy(t => t.NameVi).ToListAsync();
    //    if (themes.Count < 3) return;

    //    context.StoryTemplates.AddRange(
    //        new StoryTemplate
    //        {
    //            EducationThemeId = themes[0].Id,
    //            Title = "The Brave Little Fox",
    //            Description = "A shy fox learns to be brave and help a friend.",
    //            AgeGroup = TargetAgeGroup.Ag6to7,
    //            CoverImageUrl = "/seed/covers/brave-little-fox.png",
    //            TotalPageNumbers = 2,
    //            Status = CommonStatus.Active,
    //            CreateBy = SystemUserId
    //        },
    //        new StoryTemplate
    //        {
    //            EducationThemeId = themes[1].Id,
    //            Title = "Sharing Picnic",
    //            Description = "Friends learn that sharing makes the picnic more fun.",
    //            AgeGroup = TargetAgeGroup.Age8to10,
    //            CoverImageUrl = "/seed/covers/sharing-picnic.png",
    //            TotalPageNumbers = 2,
    //            Status = CommonStatus.Active,
    //            CreateBy = SystemUserId
    //        },
    //        new StoryTemplate
    //        {
    //            EducationThemeId = themes[2].Id,
    //            Title = "Garden Helpers",
    //            Description = "Tiny garden friends take care of plants together.",
    //            AgeGroup = TargetAgeGroup.Age8to10,
    //            CoverImageUrl = "/seed/covers/garden-helpers.png",
    //            TotalPageNumbers = 2,
    //            Status = CommonStatus.Active,
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedStoryRoleTemplatesAsync(AppDbContext context)
    //{
    //    if (await context.StoryRoleTemplates.AnyAsync()) return;

    //    var stories = await context.StoryTemplates.OrderBy(s => s.Title).ToListAsync();
    //    if (stories.Count < 2) return;

    //    context.StoryRoleTemplates.AddRange(
    //        new StoryRoleTemplate
    //        {
    //            StoryTemplateId = stories[0].Id,
    //            StoryRoleType = StoryRoleType.MainCharacter,
    //            RoleName = "Hero",
    //            Description = "The main child-created hero of the story.",
    //            AllowCustomCharacter = true,
    //            RequiresParentApproval = false,
    //            DefaultAssetUrl = "/seed/roles/hero.png",
    //            Status = CommonStatus.Active,
    //            CreateBy = SystemUserId
    //        },
    //        new StoryRoleTemplate
    //        {
    //            StoryTemplateId = stories[0].Id,
    //            StoryRoleType = StoryRoleType.Companion,
    //            RoleName = "Best Friend",
    //            Description = "A loyal companion who helps the hero.",
    //            AllowCustomCharacter = true,
    //            RequiresParentApproval = true,
    //            DefaultAssetUrl = "/seed/roles/friend.png",
    //            Status = CommonStatus.Active,
    //            CreateBy = SystemUserId
    //        },
    //        new StoryRoleTemplate
    //        {
    //            StoryTemplateId = stories[1].Id,
    //            StoryRoleType = StoryRoleType.MainCharacter,
    //            RoleName = "Explorer",
    //            Description = "Leads the adventure through the story world.",
    //            AllowCustomCharacter = true,
    //            RequiresParentApproval = false,
    //            DefaultAssetUrl = "/seed/roles/explorer.png",
    //            Status = CommonStatus.Active,
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedStoryPageTemplatesAsync(AppDbContext context)
    //{
    //    if (await context.StoryPageTemplates.AnyAsync()) return;

    //    var stories = await context.StoryTemplates.OrderBy(s => s.Title).ToListAsync();
    //    if (stories.Count < 2) return;

    //    context.StoryPageTemplates.AddRange(
    //        new StoryPageTemplate
    //        {
    //            StoryTemplateId = stories[0].Id,
    //            PageNumber = 1,
    //            RawText = "Once upon a time, a little hero looked at a tall hill and felt a tiny bit scared.",
    //            BackgroundUrl = "/seed/pages/hill.png",
    //            AudioNarrationUrl = "/seed/audio/page-1.mp3",
    //            Status = CommonStatus.Active,
    //            CreateBy = SystemUserId
    //        },
    //        new StoryPageTemplate
    //        {
    //            StoryTemplateId = stories[0].Id,
    //            PageNumber = 2,
    //            RawText = "With a friend beside them, the hero took one small step and then another.",
    //            BackgroundUrl = "/seed/pages/path.png",
    //            AudioNarrationUrl = "/seed/audio/page-2.mp3",
    //            Status = CommonStatus.Active,
    //            CreateBy = SystemUserId
    //        },
    //        new StoryPageTemplate
    //        {
    //            StoryTemplateId = stories[1].Id,
    //            PageNumber = 1,
    //            RawText = "The explorer packed a tiny bag and set out to discover a secret garden.",
    //            BackgroundUrl = "/seed/pages/garden.png",
    //            AudioNarrationUrl = "/seed/audio/page-3.mp3",
    //            Status = CommonStatus.Active,
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedCharacterSlotTemplatesAsync(AppDbContext context)
    //{
    //    if (await context.CharacterSlotTemplates.AnyAsync()) return;

    //    var firstStory = await context.StoryTemplates.OrderBy(s => s.Title).FirstOrDefaultAsync();
    //    if (firstStory is null) return;

    //    var firstStoryPages = await context.StoryPageTemplates
    //        .Where(p => p.StoryTemplateId == firstStory.Id)
    //        .OrderBy(p => p.PageNumber)
    //        .ToListAsync();
    //    var firstStoryRoles = await context.StoryRoleTemplates
    //        .Where(r => r.StoryTemplateId == firstStory.Id)
    //        .OrderBy(r => r.RoleName)
    //        .ToListAsync();
    //    if (firstStoryPages.Count < 2 || firstStoryRoles.Count < 2) return;

    //    context.CharacterSlotTemplates.AddRange(
    //        new CharacterSlotTemplate
    //        {
    //            StoryPageId = firstStoryPages[0].Id,
    //            StoryRoleTemplateId = firstStoryRoles[0].Id,
    //            PosX = 120,
    //            PosY = 180,
    //            Scale = 1.00f,
    //            FlipHorizontal = false,
    //            Status = CommonStatus.Active,
    //            CreateBy = SystemUserId
    //        },
    //        new CharacterSlotTemplate
    //        {
    //            StoryPageId = firstStoryPages[1].Id,
    //            StoryRoleTemplateId = firstStoryRoles[0].Id,
    //            PosX = 90,
    //            PosY = 200,
    //            Scale = 1.10f,
    //            FlipHorizontal = true,
    //            Status = CommonStatus.Active,
    //            CreateBy = SystemUserId
    //        },
    //        new CharacterSlotTemplate
    //        {
    //            StoryPageId = firstStoryPages[1].Id,
    //            StoryRoleTemplateId = firstStoryRoles[1].Id,
    //            PosX = 260,
    //            PosY = 210,
    //            Scale = 0.95f,
    //            FlipHorizontal = false,
    //            Status = CommonStatus.Active,
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedVocabularyTemplatesAsync(AppDbContext context)
    //{
    //    if (await context.VocabularyTemplates.AnyAsync()) return;

    //    var pages = await context.StoryPageTemplates.OrderBy(p => p.PageNumber).ToListAsync();
    //    if (pages.Count < 2) return;

    //    context.VocabularyTemplates.AddRange(
    //        new VocabularyTemplate
    //        {
    //            StoryPageTemplateId = pages[0].Id,
    //            Word = "brave",
    //            Meaning = "Ready to try even when you feel a little scared.",
    //            AudioUrl = "/seed/vocab/brave.mp3",
    //            CreateBy = SystemUserId
    //        },
    //        new VocabularyTemplate
    //        {
    //            StoryPageTemplateId = pages[1].Id,
    //            Word = "friend",
    //            Meaning = "Someone who cares about you and helps you.",
    //            AudioUrl = "/seed/vocab/friend.mp3",
    //            CreateBy = SystemUserId
    //        },
    //        new VocabularyTemplate
    //        {
    //            StoryPageTemplateId = pages[0].Id,
    //            Word = "hill",
    //            Meaning = "A small mountain you can climb.",
    //            AudioUrl = "/seed/vocab/hill.mp3",
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedStoryQuizTemplatesAsync(AppDbContext context)
    //{
    //    if (await context.StoryQuizTemplates.AnyAsync()) return;

    //    var stories = await context.StoryTemplates.OrderBy(s => s.Title).ToListAsync();
    //    if (stories.Count < 2) return;

    //    context.StoryQuizTemplates.AddRange(
    //        new StoryQuizTemplate
    //        {
    //            StoryTemplateId = stories[0].Id,
    //            QuestionText = "How did the hero start climbing the hill?",
    //            QuestionAudioUrl = "/seed/quiz/q1.mp3",
    //            QuestionPicUrl = "/seed/quiz/q1.png",
    //            OptionsJson = "[\"Took one small step\",\"Gave up\",\"Waited forever\"]",
    //            CorrectOptionIndex = 0,
    //            Status = CommonStatus.Active,
    //            CreateBy = SystemUserId
    //        },
    //        new StoryQuizTemplate
    //        {
    //            StoryTemplateId = stories[0].Id,
    //            QuestionText = "Who helped the hero feel braver?",
    //            QuestionAudioUrl = "/seed/quiz/q2.mp3",
    //            OptionsJson = "[\"A stranger\",\"A best friend\",\"Nobody\"]",
    //            CorrectOptionIndex = 1,
    //            Status = CommonStatus.Active,
    //            CreateBy = SystemUserId
    //        },
    //        new StoryQuizTemplate
    //        {
    //            StoryTemplateId = stories[1].Id,
    //            QuestionText = "What did the explorer pack for the garden trip?",
    //            QuestionAudioUrl = "/seed/quiz/q3.mp3",
    //            OptionsJson = "[\"A tiny bag\",\"A giant truck\",\"Nothing\"]",
    //            CorrectOptionIndex = 0,
    //            Status = CommonStatus.Active,
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedDrawingsAsync(AppDbContext context)
    //{
    //    if (await context.Drawings.AnyAsync()) return;

    //    var children = await context.ChildProfiles.OrderBy(c => c.NickName).ToListAsync();
    //    if (children.Count < 2) return;

    //    context.Drawings.AddRange(
    //        new Drawing
    //        {
    //            ChildProfileId = children[0].Id,
    //            OriginalImageUrl = "/seed/drawings/fox.png",
    //            DrawingType = DrawingType.Canvas,
    //            CreateBy = SystemUserId
    //        },
    //        new Drawing
    //        {
    //            ChildProfileId = children[0].Id,
    //            OriginalImageUrl = "/seed/drawings/buddy.png",
    //            DrawingType = DrawingType.Uploaded,
    //            CreateBy = SystemUserId
    //        },
    //        new Drawing
    //        {
    //            ChildProfileId = children[1].Id,
    //            OriginalImageUrl = "/seed/drawings/explorer.png",
    //            DrawingType = DrawingType.Canvas,
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedAICharacterGeneratesAsync(AppDbContext context)
    //{
    //    if (await context.AICharacterGenerates.AnyAsync()) return;

    //    var children = await context.ChildProfiles.OrderBy(c => c.NickName).ToListAsync();
    //    var drawings = await context.Drawings.OrderBy(d => d.CreatedAt).ToListAsync();
    //    var charTypes = await context.CharTypes.OrderBy(t => t.NameEn).ToListAsync();
    //    var child0Drawings = drawings.Where(d => d.ChildProfileId == children[0].ChildProfileId).ToList();
    //    var child1Drawings = drawings.Where(d => d.ChildProfileId == children[1].ChildProfileId).ToList();
    //    if (children.Count < 2 || child0Drawings.Count < 2 || child1Drawings.Count < 1 || charTypes.Count < 3) return;

    //    context.AICharacterGenerates.AddRange(
    //        new AICharacterGenerate
    //        {
    //            ChildProfileId = children[0].Id,
    //            DrawingId = child0Drawings[0].Id,
    //            CharTypeId = charTypes[0].Id,
    //            AIPromptUsed = "Turn this drawing into a friendly fox hero, child-safe, bright colors.",
    //            GenerateImageUrl = "/seed/ai/fox-hero.png",
    //            AIDetectedTagsJson = "[\"fox\",\"hero\",\"kind\"]",
    //            AIGenStatus = AIGenStatus.Success,
    //            CreateBy = SystemUserId
    //        },
    //        new AICharacterGenerate
    //        {
    //            ChildProfileId = children[0].Id,
    //            DrawingId = child0Drawings[1].Id,
    //            CharTypeId = charTypes[2].Id,
    //            AIPromptUsed = "Turn this drawing into a cheerful companion animal.",
    //            GenerateImageUrl = "/seed/ai/buddy.png",
    //            AIDetectedTagsJson = "[\"buddy\",\"animal\",\"smile\"]",
    //            AIGenStatus = AIGenStatus.Success,
    //            CreateBy = SystemUserId
    //        },
    //        new AICharacterGenerate
    //        {
    //            ChildProfileId = children[1].Id,
    //            DrawingId = child1Drawings[0].Id,
    //            CharTypeId = charTypes[1].Id,
    //            AIPromptUsed = "Turn this drawing into a curious child explorer.",
    //            GenerateImageUrl = "/seed/ai/explorer.png",
    //            AIDetectedTagsJson = "[\"explorer\",\"human\",\"curious\"]",
    //            AIGenStatus = AIGenStatus.Success,
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedCharactersAsync(AppDbContext context)
    //{
    //    if (await context.Characters.AnyAsync()) return;

    //    var generations = await context.AICharacterGenerates.OrderBy(g => g.GenerateImageUrl).ToListAsync();
    //    if (generations.Count < 3) return;

    //    context.Characters.AddRange(
    //        new Character
    //        {
    //            ChildProfileId = generations[0].ChildProfileId,
    //            AICharacterGenerateId = generations[0].Id,
    //            Name = "Foxy",
    //            DefaultPronoun = "they",
    //            FinalImageUrl = generations[0].GenerateImageUrl ?? "/seed/ai/fox-hero.png",
    //            Status = CharacterStatus.Approved,
    //            IsFavorite = true,
    //            CreateBy = SystemUserId
    //        },
    //        new Character
    //        {
    //            ChildProfileId = generations[1].ChildProfileId,
    //            AICharacterGenerateId = generations[1].Id,
    //            Name = "Buddy",
    //            DefaultPronoun = "he",
    //            FinalImageUrl = generations[1].GenerateImageUrl ?? "/seed/ai/buddy.png",
    //            Status = CharacterStatus.Approved,
    //            IsFavorite = false,
    //            CreateBy = SystemUserId
    //        },
    //        new Character
    //        {
    //            ChildProfileId = generations[2].ChildProfileId,
    //            AICharacterGenerateId = generations[2].Id,
    //            Name = "Mika",
    //            DefaultPronoun = "she",
    //            FinalImageUrl = generations[2].GenerateImageUrl ?? "/seed/ai/explorer.png",
    //            Status = CharacterStatus.Approved,
    //            IsFavorite = true,
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedGeneratedStoriesAsync(AppDbContext context)
    //{
    //    if (await context.GeneratedStories.AnyAsync()) return;

    //    var children = await context.ChildProfiles.OrderBy(c => c.NickName).ToListAsync();
    //    var stories = await context.StoryTemplates.OrderBy(s => s.Title).ToListAsync();
    //    if (children.Count < 2 || stories.Count < 2) return;

    //    context.GeneratedStories.AddRange(
    //        new GeneratedStory
    //        {
    //            ChildProfileId = children[0].Id,
    //            StoryTemplateId = stories[0].Id,
    //            IsFavorite = true,
    //            Status = GeneratedStoryStatus.Completed,
    //            LastPageRead = 2,
    //            CreateBy = SystemUserId
    //        },
    //        new GeneratedStory
    //        {
    //            ChildProfileId = children[0].Id,
    //            StoryTemplateId = stories[1].Id,
    //            IsFavorite = false,
    //            Status = GeneratedStoryStatus.InProgress,
    //            LastPageRead = 1,
    //            CreateBy = SystemUserId
    //        },
    //        new GeneratedStory
    //        {
    //            ChildProfileId = children[1].Id,
    //            StoryTemplateId = stories[1].Id,
    //            IsFavorite = true,
    //            Status = GeneratedStoryStatus.InProgress,
    //            LastPageRead = 1,
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedUserStoryCharacterMappingsAsync(AppDbContext context)
    //{
    //    if (await context.UserStoryCharacterMappings.AnyAsync()) return;

    //    var generatedStories = await context.GeneratedStories.OrderBy(s => s.CreatedAt).ToListAsync();
    //    var roles = await context.StoryRoleTemplates.ToListAsync();
    //    var characters = await context.Characters.ToListAsync();
    //    if (generatedStories.Count == 0) return;

    //    var firstStory = generatedStories[0];
    //    var firstStoryRoles = roles.Where(r => r.StoryTemplateId == firstStory.StoryTemplateId).OrderBy(r => r.RoleName).ToList();
    //    var firstChildChars = characters.Where(c => c.ChildProfileId == firstStory.ChildProfileId).OrderBy(c => c.Name).ToList();
    //    if (firstStoryRoles.Count == 0 || firstChildChars.Count == 0) return;

    //    context.UserStoryCharacterMappings.Add(
    //        new UserStoryCharacterMapping
    //        {
    //            GeneratedStoryId = firstStory.Id,
    //            StoryRoleTemplateId = firstStoryRoles[0].Id,
    //            CharacterId = firstChildChars[0].Id,
    //            ParentApprovalStatus = ParentApprovalStatus.NotRequired,
    //            CreateBy = SystemUserId
    //        });

    //    if (firstStoryRoles.Count > 1 && firstChildChars.Count > 1)
    //    {
    //        context.UserStoryCharacterMappings.Add(
    //            new UserStoryCharacterMapping
    //            {
    //                GeneratedStoryId = firstStory.Id,
    //                StoryRoleTemplateId = firstStoryRoles[1].Id,
    //                CharacterId = firstChildChars[1].Id,
    //                ParentApprovalStatus = ParentApprovalStatus.Approved,
    //                CreateBy = SystemUserId
    //            });
    //    }

    //    var secondStory = generatedStories.FirstOrDefault(s => s.Id != firstStory.Id);
    //    if (secondStory is not null)
    //    {
    //        var secondRoles = roles.Where(r => r.StoryTemplateId == secondStory.StoryTemplateId).OrderBy(r => r.RoleName).ToList();
    //        var secondChars = characters.Where(c => c.ChildProfileId == secondStory.ChildProfileId).OrderBy(c => c.Name).ToList();
    //        if (secondRoles.Count > 0 && secondChars.Count > 0)
    //        {
    //            context.UserStoryCharacterMappings.Add(
    //                new UserStoryCharacterMapping
    //                {
    //                    GeneratedStoryId = secondStory.Id,
    //                    StoryRoleTemplateId = secondRoles[0].Id,
    //                    CharacterId = secondChars[0].Id,
    //                    ParentApprovalStatus = ParentApprovalStatus.NotRequired,
    //                    CreateBy = SystemUserId
    //                });
    //        }
    //    }

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedChildVocabularyProgressesAsync(AppDbContext context)
    //{
    //    if (await context.ChildVocabularyProgresses.AnyAsync()) return;

    //    var children = await context.ChildProfiles.OrderBy(c => c.NickName).ToListAsync();
    //    var vocab = await context.VocabularyTemplates.OrderBy(v => v.Word).ToListAsync();
    //    if (children.Count == 0 || vocab.Count < 3) return;

    //    context.ChildVocabularyProgresses.AddRange(
    //        new ChildVocabularyProgress
    //        {
    //            ChildProfileId = children[0].Id,
    //            VocabularyTemplateId = vocab[0].Id,
    //            TotalListenCount = 4,
    //            TotalQuizAttempts = 2,
    //            CorrectAnswersCount = 2,
    //            WrongAnswersCount = 0,
    //            CreateBy = SystemUserId
    //        },
    //        new ChildVocabularyProgress
    //        {
    //            ChildProfileId = children[0].Id,
    //            VocabularyTemplateId = vocab[1].Id,
    //            TotalListenCount = 3,
    //            TotalQuizAttempts = 2,
    //            CorrectAnswersCount = 1,
    //            WrongAnswersCount = 1,
    //            CreateBy = SystemUserId
    //        },
    //        new ChildVocabularyProgress
    //        {
    //            ChildProfileId = children[0].Id,
    //            VocabularyTemplateId = vocab[2].Id,
    //            TotalListenCount = 1,
    //            TotalQuizAttempts = 1,
    //            CorrectAnswersCount = 0,
    //            WrongAnswersCount = 1,
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedReadingLogsAsync(AppDbContext context)
    //{
    //    if (await context.ReadingLogs.AnyAsync()) return;

    //    var generatedStories = await context.GeneratedStories.OrderBy(s => s.CreatedAt).ToListAsync();
    //    if (generatedStories.Count < 2) return;

    //    context.ReadingLogs.AddRange(
    //        new ReadingLog
    //        {
    //            ChildProfileId = generatedStories[0].ChildProfileId,
    //            GeneratedStoryId = generatedStories[0].Id,
    //            IsCompleted = true,
    //            ReadDurationSeconds = 420,
    //            CreateBy = SystemUserId
    //        },
    //        new ReadingLog
    //        {
    //            ChildProfileId = generatedStories[1].ChildProfileId,
    //            GeneratedStoryId = generatedStories[1].Id,
    //            IsCompleted = false,
    //            ReadDurationSeconds = 150,
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedChildDailyUsageLogsAsync(AppDbContext context)
    //{
    //    if (await context.ChildDailyUsageLogs.AnyAsync()) return;

    //    var children = await context.ChildProfiles.OrderBy(c => c.NickName).ToListAsync();
    //    if (children.Count < 2) return;

    //    var today = DateOnly.FromDateTime(SeedTime);
    //    context.ChildDailyUsageLogs.AddRange(
    //        new ChildDailyUsageLog
    //        {
    //            ChildProfileId = children[0].Id,
    //            LogDate = today,
    //            DurationSecond = 900,
    //            CharacterCreatedCount = 1,
    //            StoryReadCount = 1,
    //            CreateBy = SystemUserId
    //        },
    //        new ChildDailyUsageLog
    //        {
    //            ChildProfileId = children[0].Id,
    //            LogDate = today.AddDays(-1),
    //            DurationSecond = 600,
    //            CharacterCreatedCount = 1,
    //            StoryReadCount = 0,
    //            CreateBy = SystemUserId
    //        },
    //        new ChildDailyUsageLog
    //        {
    //            ChildProfileId = children[1].Id,
    //            LogDate = today,
    //            DurationSecond = 480,
    //            CharacterCreatedCount = 1,
    //            StoryReadCount = 1,
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedChildQuizAnswersAsync(AppDbContext context)
    //{
    //    if (await context.ChildQuizAnswers.AnyAsync()) return;

    //    var logs = await context.ReadingLogs.OrderBy(l => l.CreatedAt).ToListAsync();
    //    var quizzes = await context.StoryQuizTemplates.OrderBy(q => q.QuestionText).ToListAsync();
    //    if (logs.Count == 0 || quizzes.Count < 2) return;

    //    context.ChildQuizAnswers.AddRange(
    //        new ChildQuizAnswer
    //        {
    //            ChildProfileId = logs[0].ChildProfileId,
    //            ReadingLogId = logs[0].Id,
    //            StoryQuizTemplateId = quizzes[0].Id,
    //            QuizScore = 10,
    //            SelectedOptionIndex = 0,
    //            IsCorrect = true,
    //            ResponseTimeSeconds = 8,
    //            CreateBy = SystemUserId
    //        },
    //        new ChildQuizAnswer
    //        {
    //            ChildProfileId = logs[0].ChildProfileId,
    //            ReadingLogId = logs[0].Id,
    //            StoryQuizTemplateId = quizzes[1].Id,
    //            QuizScore = 0,
    //            SelectedOptionIndex = 2,
    //            IsCorrect = false,
    //            ResponseTimeSeconds = 12,
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    if (logs.Count > 1 && quizzes.Count > 2)
    //    {
    //        context.ChildQuizAnswers.Add(
    //            new ChildQuizAnswer
    //            {
    //                ChildProfileId = logs[1].ChildProfileId,
    //                ReadingLogId = logs[1].Id,
    //                StoryQuizTemplateId = quizzes[2].Id,
    //                QuizScore = 10,
    //                SelectedOptionIndex = 0,
    //                IsCorrect = true,
    //                ResponseTimeSeconds = 6,
    //                CreateBy = SystemUserId
    //            });
    //    }

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedContentReportsAsync(AppDbContext context)
    //{
    //    if (await context.ContentReports.AnyAsync()) return;

    //    var character = await context.Characters.OrderBy(c => c.Name).FirstOrDefaultAsync();
    //    var story = await context.GeneratedStories.OrderBy(s => s.CreatedAt).FirstOrDefaultAsync();
    //    if (character is null || story is null) return;

    //    context.ContentReports.AddRange(
    //        new ContentReport
    //        {
    //            TargetType = ReportTargetType.Character,
    //            TargetId = character.Id,
    //            Reason = "Character image looks confusing for young readers.",
    //            Status = ContentReportStatus.Pending,
    //            CreateBy = SystemUserId
    //        },
    //        new ContentReport
    //        {
    //            TargetType = ReportTargetType.Story,
    //            TargetId = story.Id,
    //            Reason = "Story wording may be too advanced for the age group.",
    //            Status = ContentReportStatus.Processing,
    //            CreateBy = SystemUserId
    //        },
    //        new ContentReport
    //        {
    //            TargetType = ReportTargetType.Content,
    //            TargetId = story.Id,
    //            Reason = "Background art does not match the page text.",
    //            Status = ContentReportStatus.Resolve,
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}

    //private static async Task SeedAIUsageLogsAsync(AppDbContext context)
    //{
    //    if (await context.AIUsageLogs.AnyAsync()) return;

    //    context.AIUsageLogs.AddRange(
    //        new AIUsageLog
    //        {
    //            Provider = "OpenAI",
    //            ActionType = AIActionType.ClassifyImage,
    //            TokenUsed = 420,
    //            CostAmount = 0.012f,
    //            CreateBy = SystemUserId
    //        },
    //        new AIUsageLog
    //        {
    //            Provider = "OpenAI",
    //            ActionType = AIActionType.CharacterGenerate,
    //            TokenUsed = 1800,
    //            CostAmount = 0.085f,
    //            CreateBy = SystemUserId
    //        },
    //        new AIUsageLog
    //        {
    //            Provider = "Gemini",
    //            ActionType = AIActionType.CharacterGenerate,
    //            TokenUsed = 1500,
    //            CostAmount = 0.062f,
    //            CreateBy = SystemUserId
    //        }
    //    );

    //    await context.SaveChangesAsync();
    //}
}
