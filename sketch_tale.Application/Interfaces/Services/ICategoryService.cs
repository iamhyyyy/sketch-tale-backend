using sketch_tale.Application.DTOs;

namespace sketch_tale.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(Guid id);
        Task<CategoryDto> AddAsync(CreateCategoryDto dto);
        Task<bool> Update(Guid id, UpdateCategoryDto dto);
    }
}