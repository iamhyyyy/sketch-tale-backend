using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.DTOs
{
    public class ParentProfileDto : BaseFieldDto
    {
        public Guid UserId { get; set; }

        public int ChildProfileLimit { get; set; }
        public int RemainingChildProfileLimit { get; set; }

        public int MonthlyCreditLimit { get; set; }
        public int RemainingMonthlyCreditLimit { get; set; }

        public bool CanExportStory { get; set; }
        public bool AccessFullStories { get; set; }
        public bool SubscriptionTrialPlusPlan { get; set; }

        public string SecurityCode { get; set; } = string.Empty;

        // Thuộc tính tùy chỉnh
        public string CurrentPlan { get; set; } = "Free";

        // Các bảng liên kết (Navigation Properties)
        //public ParentProfileSubDto? ParentProfileSub { get; set; }
        public ICollection<ChildProfileDto> ChildProfiles { get; set; } = new List<ChildProfileDto>();
    }
    public class UpdateParentProfileDto
    {
        public int ChildProfileLimit { get; set; } = 1;
        public int RemainingChildProfileLimit { get; set; } = 1;

        public int MonthlyCreditLimit { get; set; } = 500;
        public int RemainingMonthlyCreditLimit { get; set; } = 500;

        public bool CanExportStory { get; set; } = false;
        public bool AccessFullStories { get; set; } = false;
        public bool SubscriptionTrialPlusPlan { get; set; } = false;

        [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã bảo mật phải bao gồm đúng 6 chữ số.")]
        public string SecurityCode { get; set; } = "000000";
    }
}
