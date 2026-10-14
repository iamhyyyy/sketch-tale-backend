using sketch_tale.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Interfaces.Services
{
    public interface IChildProfileService
    {
        Task<IEnumerable<ChildProfileDto>> GetAllChildProfilesAsync();
        Task<ChildProfileDto?> GetByIdAsync(Guid id);
        Task<ParentProfileDto?> GetByParentProfileIdAsync(Guid parentProfileId);
        Task<bool> UpdateAsync(Guid id, UpdateChildProfileDto dto);
        //Task<bool> DeleteAsync(Guid id);
        Task<string?> GetNickNameByIdAsync(Guid id);
        Task<int?> GetDailyTimeLimitByIdAsync(Guid id);       // Kiểu int (hoặc TimeSpan tùy thuộc vào thiết kế DB của bạn)
        Task<int?> GetDailyCharacterLimitByIdAsync(Guid id);   // Kiểu int cho giới hạn ký tự/lượt tạo mỗi ngày
        Task<string?> GetTargetAgeGroupByIdAsync(Guid id);     // Kiểu string (vd: "3-5", "6-8") hoặc enum tùy hệ thống
    }
}
