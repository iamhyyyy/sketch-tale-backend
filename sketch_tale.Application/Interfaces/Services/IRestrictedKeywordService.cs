using sketch_tale.Application.DTOs;

namespace sketch_tale.Application.Interfaces
{
    public interface IRestrictedKeywordService
    {
        Task<IEnumerable<RestrictedKeywordDto>> GetAllAsync();
        Task<RestrictedKeywordDto?> GetByIdAsync(Guid id);
        Task<RestrictedKeywordDto> AddAsync(CreateRestrictedKeywordDto dto);
        Task<bool> Update(Guid id, UpdateRestrictedKeywordDto dto);
        Task<bool> Delete(Guid id);
    }
}