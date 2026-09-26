
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



    }
}
