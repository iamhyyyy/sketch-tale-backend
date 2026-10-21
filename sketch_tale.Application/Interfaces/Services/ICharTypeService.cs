using sketch_tale.Application.DTOs;

namespace sketch_tale.Application.Interfaces
{
    public interface ICharTypeService
    {
        Task<IEnumerable<CharTypeDto>> GetAllAsync();
        Task<CharTypeDto?> GetByIdAsync(Guid id);
        Task<CharTypeDto> AddAsync(CreateCharTypeDto dto);
        Task<bool> Update(Guid id, UpdateCharTypeDto dto);
        Task<bool> Delete(Guid id);
    }
}