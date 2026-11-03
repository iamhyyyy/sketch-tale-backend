using sketch_tale.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Repositories;

public interface IChildQuizAnswerRepository : IGenericRepository<ChildQuizAnswer>
{
    Task<IEnumerable<ChildQuizAnswer>> GetAnswersByReadingLogIdAsync(Guid readingLogId);
    Task<ChildQuizAnswer?> GetAnswerAsync(Guid readingLogId, Guid storyQuizTemplateId);
    Task<IEnumerable<ChildQuizAnswer>> GetAnswersByChildProfileIdAsync(Guid childProfileId);
}
