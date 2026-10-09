using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace sketch_tale.Domain.Entities;

public class ParentProfile : BaseEntity
{
    public Guid UserId { get; set; }

    public int ChildProfileLimit { get; set; } = 1;
    public int RemainingChildProfileLimit { get; set; } = 1;

    public int MonthlyCreditLimit { get; set; } = 500;
    public int RemainingMonthlyCreditLimit { get; set; } = 500;

    public bool CanExportStory { get; set; } = false;
    public bool AccessFullStories { get; set; } = false;
    public bool SubscriptionTrialPlusPlan { get; set; } = false;

    [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã bảo mật phải bao gồm đúng 6 chữ số.")]
    public string SecurityCode { get; set; } = "000000";

    public ParentProfileSub? ParentProfileSub { get; set; }
}
