using sketch_tale.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Repositories
{
    public interface IParentProfileRepository : IGenericRepository<ParentProfile>
    {
        Task<IEnumerable<ParentProfile>> GetAllWithDetailsAsync();
        Task<ParentProfile?> GetByIdWithDetailsAsync(Guid id);
    }
}
