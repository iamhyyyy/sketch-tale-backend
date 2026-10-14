using sketch_tale.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Services
{
    public interface IParentProfileService
    {
        /// Các hàm CRUD cơ bản
        Task<IEnumerable<ParentProfileDto>> GetAllAsync();
        Task<ParentProfileDto?> GetByIdAsync(Guid id);
        
        Task<bool> UpdateAsync(Guid id, UpdateParentProfileDto dto);
        

        // Các hàm nghiệp vụ riêng của Parent
        Task<bool> UpdateProfileForSubscriptionAsync(Guid id, int childProfileLimit, int monthlyCreditLimit, bool canExportStory, bool accessFullStories);
        Task<bool> MarkTrialPlanUsedAsync(Guid id);
    }
}
