
using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // EducationTheme
        CreateMap<EducationTheme, EducationThemeDto>().ReverseMap();
        CreateMap<CreateEducationThemeDto, EducationTheme>();
        CreateMap<UpdateEducationThemeDto, EducationTheme>();

        // CreditCost
        CreateMap<CreditCost, CreditCostDto>().ReverseMap();
        CreateMap<CreateCreditCostDto, CreditCost>();
        CreateMap<UpdateCreditCostDto, CreditCost>();

        // SubscriptionPlan
        CreateMap<SubscriptionPlan, SubscriptionPlanDto>().ReverseMap();
        CreateMap<CreateSubscriptionPlanDto, SubscriptionPlan>();
        CreateMap<UpdateSubscriptionPlanDto, SubscriptionPlan>();

        // Subscription
        CreateMap<Subscription, SubscriptionDto>().ReverseMap();
        CreateMap<CreateSubscriptionDto, Subscription>();
        CreateMap<UpdateSubscriptionDto, Subscription>();

        // Auth & User
        CreateMap<RegisterParentDto, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        CreateMap<RegisterChildDto, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        CreateMap<User, UserProfileDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.ParentProfile, opt => opt.Ignore())
            .ForMember(dest => dest.ChildProfile, opt => opt.Ignore());

        // ParentProfile & ChildProfile
        CreateMap<ParentProfile, ParentProfileDto>()
            .ForMember(dest => dest.CurrentPlan, opt => opt.Ignore())
            .ReverseMap();
        //StoryTemplate
        CreateMap<StoryTemplate, StoryTemplateDto>();
        CreateMap<CreateStoryTemplateDto, StoryTemplate>();
        CreateMap<UpdateStoryTemplateDto, StoryTemplate>();
        //Story_PAGE_Template
        CreateMap<CreateStoryPageTemplateDto, StoryPageTemplate>();
        CreateMap<UpdateStoryPageTemplateDto, StoryPageTemplate>();
        CreateMap<StoryPageTemplate, StoryPageTemplateDto>();
        //Story_ROLE_Template
        CreateMap<CreateStoryRoleTemplateDto, StoryRoleTemplate>();
        CreateMap<UpdateStoryRoleTemplateDto, StoryRoleTemplate>();
        CreateMap<StoryRoleTemplate, StoryRoleTemplateDto>();
        //Character_SLOT_Template
        CreateMap<CreateCharacterSlotTemplateDto, CharacterSlotTemplate>();
        CreateMap<UpdateCharacterSlotTemplateDto, CharacterSlotTemplate>();
        CreateMap<CharacterSlotTemplate, CharacterSlotTemplateDto>();

        //Story_QUIZ_Template
        CreateMap<CreateStoryQuizTemplateDto, StoryQuizTemplate>()
            .ForMember(dest => dest.OptionsJson, opt => opt.MapFrom(src => System.Text.Json.JsonSerializer.Serialize(src.Options, (System.Text.Json.JsonSerializerOptions?)null)));
        CreateMap<UpdateStoryQuizTemplateDto, StoryQuizTemplate>()
            .ForMember(dest => dest.OptionsJson, opt => opt.MapFrom(src => System.Text.Json.JsonSerializer.Serialize(src.Options, (System.Text.Json.JsonSerializerOptions?)null)));
        CreateMap<StoryQuizTemplate, StoryQuizTemplateDto>()
            .AfterMap((src, dest) =>
            {
                if (!string.IsNullOrWhiteSpace(src.OptionsJson))
                {
                    try
                    {
                        dest.Options = System.Text.Json.JsonSerializer.Deserialize<List<string>>(src.OptionsJson) ?? new List<string>();
                    }
                    catch
                    {
                        dest.Options = new List<string>();
                    }
                }
            });

        //Vocabulary_Template
        CreateMap<CreateVocabularyTemplateDto, VocabularyTemplate>();
        CreateMap<UpdateVocabularyTemplateDto, VocabularyTemplate>();
        CreateMap<VocabularyTemplate, VocabularyTemplateDto>();

        CreateMap<ChildProfile, ChildProfileDto>()
            .ForMember(dest => dest.TargetAgeGroup, opt => opt.MapFrom(src => src.TargetAgeGroup.ToString()));

        CreateMap<RegisterChildDto, ChildProfile>();
        //ChildProfile
        CreateMap<ChildProfile, ChildProfileDto>()
            .ForMember(dest => dest.TargetAgeGroup, opt => opt.MapFrom(src => src.TargetAgeGroup.ToString()))
            .ForMember(dest => dest.ReadingLevel, opt => opt.MapFrom(src => src.ReadingLevel.ToString()))       
            .ReverseMap()
            .ForMember(dest => dest.TargetAgeGroup, opt => opt.MapFrom(src => Enum.Parse<TargetAgeGroup>(src.TargetAgeGroup)))
            .ForMember(dest => dest.ReadingLevel, opt => opt.MapFrom(src => Enum.Parse<ReadingLevel>(src.ReadingLevel)));
        CreateMap<UpdateChildProfileDto, ChildProfile>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        //ParentProfile
        CreateMap<ParentProfile, ParentProfileDto>()
            .ForMember(dest => dest.CurrentPlan, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<UpdateParentProfileDto, ParentProfile>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
