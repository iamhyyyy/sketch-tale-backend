using System;
using System.Collections.Generic;

namespace sketch_tale.Application.DTOs
{
    public class ChildProfileDto : BaseFieldDto
    {
        public Guid UserId { get; set; }
        public Guid ParentProfileId { get; set; }
        public string NickName { get; set; } = string.Empty;
        public string TargetAgeGroup { get; set; } = string.Empty;
        public string ReadingLevel { get; set; } = string.Empty;
        public int DailyTimeLimit { get; set; }
        public int RemainingTimeLimit { get; set; }
        public int DailyCharacterLimit { get; set; }
        public int RemainingCharacterLimit { get; set; }
    }

    public class UpdateChildProfileDto
    {
        public string? NickName { get; set; }
        public string? TargetAgeGroup { get; set; }
        public string? ReadingLevel { get; set; }
        public int? DailyTimeLimit { get; set; }
        public int? RemainingTimeLimit { get; set; }
        public int? DailyCharacterLimit { get; set; }
        public int? RemainingCharacterLimit { get; set; }
    }
}
