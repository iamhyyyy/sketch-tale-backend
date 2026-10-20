
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Interfaces;
using sketch_tale.Infrastructure.Data;

namespace sketch_tale.Infrastructure.Repositories
{
    public class ProhibitedThemeRepository : GenericRepository<ProhibitedTheme>, IProhibitedThemeRepository
    {
        public ProhibitedThemeRepository(AppDbContext context) : base(context)
        {
        }

        
    }
}