using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.DTOs
{
    public class ChildProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ParentProfileId { get; set; }
        public string NickName { get; set; } = string.Empty;
        public string TargetAgeGroup { get; set; } = string.Empty;
        public string ReadingLevel { get; set; } = string.Empty;
        public int DailyTimeLimit { get; set; }
        public int DailyCharacterLimit { get; set; }
        public int RemainingCharacterLimit { get; set; }
    }
    public class UpdateChildProfileDto
    {
        public string NickName { get; set; } = string.Empty;
        public string TargetAgeGroup { get; set; } = string.Empty;
        public string ReadingLevel { get; set; } = string.Empty;
        public int DailyTimeLimit { get; set; }
        public int DailyCharacterLimit { get; set; }
        public int RemainingCharacterLimit { get; set; }
    }
}
