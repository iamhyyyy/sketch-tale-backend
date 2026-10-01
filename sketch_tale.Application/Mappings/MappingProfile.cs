
using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        //EducationTheme
        CreateMap<EducationTheme, EducationThemeDto>().ReverseMap();
        CreateMap<CreateEducationThemeDto, EducationTheme>();
        CreateMap<UpdateEducationThemeDto, EducationTheme>();

        CreateMap<RegisterParentDto, User>();
    //.ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username));
    //    // Thêm các trường khác nếu DTO và Entity có tên thuộc tính lệch nhau (ví dụ Email, v.v.)

    }
}
