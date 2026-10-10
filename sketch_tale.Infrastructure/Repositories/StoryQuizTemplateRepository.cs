using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Domain.Entities;
using sketch_tale.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Infrastructure.Repositories;

public class StoryQuizTemplateRepository : GenericRepository<StoryQuizTemplate>, IStoryQuizTemplateRepository
{
    public StoryQuizTemplateRepository(AppDbContext context) : base(context)
    {
    }
}
