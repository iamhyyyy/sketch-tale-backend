using sketch_tale.Domain.Entities;

namespace sketch_tale.Application.Interfaces.Repositories;

public interface ICharacterSlotTemplateRepository : IGenericRepository<CharacterSlotTemplate>
{
    Task<IEnumerable<CharacterSlotTemplate>> GetSlotsByPageIdAsync(Guid pageId);
}