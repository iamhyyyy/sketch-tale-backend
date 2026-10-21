using sketch_tale.Application.DTOs;
using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Interfaces
{
    public interface IUserStoryCharacterMappingService
    {
        Task<IEnumerable<UserStoryCharacterMappingDto>> GetAllAsync();
        Task<UserStoryCharacterMappingDto?> GetByIdAsync(Guid id);
        Task<List<UserStoryCharacterMappingDto>> GetByCharacterIdAsync(Guid charId);
        Task<List<UserStoryCharacterMappingDto>> GetByGenerateStoryIdAsync(Guid genStoryId);
        Task<UserStoryCharacterMappingDto> AddAsync(CreateUserStoryCharacterMappingDto dto);
        Task<bool> Update(Guid id, UpdateUserStoryCharacterMappingDto dto);
        Task<bool> Delete(Guid id);
    }
}