using sketch_tale.Application.DTOs;

namespace sketch_tale.Application.Interfaces
{
    public interface IProhibitedThemeService
    {
        Task<IEnumerable<ProhibitedThemeDto>> GetAllAsync();
        Task<ProhibitedThemeDto?> GetByIdAsync(Guid id);
        Task<ProhibitedThemeDto> AddAsync(CreateProhibitedThemeDto dto);
        Task<bool> Update(Guid id, UpdateProhibitedThemeDto dto);
        Task<bool> Delete(Guid id);
    }
}