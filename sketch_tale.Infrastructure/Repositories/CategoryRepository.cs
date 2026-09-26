
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Interfaces;
using sketch_tale.Infrastructure.Data;

namespace sketch_tale.Infrastructure.Repositories
{
    public class EducationThemeRepository : GenericRepository<EducationTheme>, IEducationThemeRepository
    {
        public EducationThemeRepository(AppDbContext context) : base(context)
        {
        }

        
    }
}