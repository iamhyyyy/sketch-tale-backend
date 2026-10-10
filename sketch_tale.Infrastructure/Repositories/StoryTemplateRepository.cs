using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Interfaces;
using sketch_tale.Infrastructure.Data;

namespace sketch_tale.Infrastructure.Repositories;

public class StoryTemplateRepository : GenericRepository<StoryTemplate>, IStoryTemplateRepository
{
    public StoryTemplateRepository(AppDbContext context) : base(context)
    {
    }
}
