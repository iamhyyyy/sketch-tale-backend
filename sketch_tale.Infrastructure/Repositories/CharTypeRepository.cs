
using Microsoft.EntityFrameworkCore;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using sketch_tale.Domain.Interfaces;
using sketch_tale.Infrastructure.Data;

namespace sketch_tale.Infrastructure.Repositories
{
    public class CharTypeRepository : GenericRepository<CharType>, ICharTypeRepository
    {
        public CharTypeRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<CharType>> GetActiveCharTypesAsync()
        {
            return await _context.CharTypes
                .Where(x => x.Status == CommonStatus.Active)
                .ToListAsync();
        }

        public async Task<string> GetCharTypeNameAsync(Guid charTypeId)
        {
            var charType = await _context.CharTypes.FindAsync(charTypeId);
            return charType?.NameEn ?? "Nhân vật";
        }
    }
}