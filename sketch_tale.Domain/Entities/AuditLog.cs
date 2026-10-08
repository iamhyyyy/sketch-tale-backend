using sketch_tale.Domain.Common;
using sketch_tale.Domain.Enums;

namespace sketch_tale.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string Action { get; set; } = null!;          // Hành động: Create, Update, Delete
    public string EntityName { get; set; } = null!;      // Bảng nào: UserProfile, ChildProfile,...
    public Guid PrimaryKey { get; set; }      // ID của bản ghi bị thay đổi

    public string? OldValues { get; set; }      // Dạng JSON lưu giá trị CŨ
    public string? NewValues { get; set; }      // Dạng JSON lưu giá trị MỚI
    public string? ChangedColumns { get; set; } // Danh sách các cột bị sửa (VD: "FullName, Age")
}
