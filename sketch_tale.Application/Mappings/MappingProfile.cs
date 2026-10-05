
using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // EducationTheme
        CreateMap<EducationTheme, EducationThemeDto>().ReverseMap();
        CreateMap<CreateEducationThemeDto, EducationTheme>();
        CreateMap<UpdateEducationThemeDto, EducationTheme>();

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
        CreateMap<ParentProfile, ParentProfileDto>().ReverseMap();

        CreateMap<ChildProfile, ChildProfileDto>()
            .ForMember(dest => dest.TargetAgeGroup, opt => opt.MapFrom(src => src.TargetAgeGroup.ToString()));

        CreateMap<RegisterChildDto, ChildProfile>();
    }
}
