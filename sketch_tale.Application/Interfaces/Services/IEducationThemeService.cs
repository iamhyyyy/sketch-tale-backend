using sketch_tale.Application.DTOs;

namespace sketch_tale.Application.Interfaces
{
    public interface IEducationThemeService
    {
        Task<IEnumerable<EducationThemeDto>> GetAllAsync();
        Task<EducationThemeDto?> GetByIdAsync(Guid id);
        Task<EducationThemeDto> AddAsync(CreateEducationThemeDto dto);
        Task<bool> Update(Guid id, UpdateEducationThemeDto dto);
    }
}