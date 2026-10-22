using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace sketch_tale.Application.DTOs
{
    public class ParentProfileDto : BaseFieldDto
    {
        public Guid UserId { get; set; }
        public Guid SubscriptionPlanId { get; set; }

        public int RemainingChildProfile { get; set; }
        public int RemainingMonthlyCredit { get; set; }
        public bool TrialPlusPlan { get; set; }

        // Thuộc tính tùy chỉnh & bảng liên kết (Navigation Properties)
        public string CurrentPlan { get; set; } = "Free";
        public SubscriptionPlanDto? SubscriptionPlan { get; set; }
        public ICollection<ChildProfileDto> ChildProfiles { get; set; } = new List<ChildProfileDto>();
    }

    public class UpdateParentProfileDto
    {
        [Required(ErrorMessage = "SubscriptionPlanId là bắt buộc.")]
        public Guid SubscriptionPlanId { get; set; }
    }
}
