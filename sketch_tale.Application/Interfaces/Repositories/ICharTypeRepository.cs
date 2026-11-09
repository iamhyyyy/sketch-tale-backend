using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Interfaces.Repositories;

public interface ICharTypeRepository : IGenericRepository<CharType>
{
    Task<List<CharType>> GetActiveCharTypesAsync();
    Task<string> GetCharTypeNameAsync(Guid charTypeId);
}