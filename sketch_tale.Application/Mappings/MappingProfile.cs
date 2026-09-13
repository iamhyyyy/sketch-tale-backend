
using AutoMapper;
using sketch_tale.Application.DTOs;
using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        //category
        CreateMap<Category, CategoryDto>().ReverseMap();
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>();



    }
}
