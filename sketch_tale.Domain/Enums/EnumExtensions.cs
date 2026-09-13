using System.Reflection;

namespace sketch_tale.Domain.Enums;

[AttributeUsage(AttributeTargets.Field)]
public sealed class DbEnumValueAttribute(string value) : Attribute
{
    public string Value { get; } = value;
}

public static class EnumExtensions
{
    //dùng để biến đổi Enum trong code thành chuỗi chữ trước khi đem cất xuống Database.
    public static string ToDbString(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString()!);
        var attr = field?.GetCustomAttribute<DbEnumValueAttribute>();
        return attr?.Value ?? value.ToString();
    }

    //hàm này sẽ nhận vào chuỗi và tìm xem trong C# ông Enum nào khớp với chuỗi đó để trả về đúng Object
    public static TEnum ParseDbString<TEnum>(string value) where TEnum : struct, Enum
    {
        foreach (var member in Enum.GetValues<TEnum>())
        {
            if (member.ToDbString().Equals(value, StringComparison.Ordinal))
                return member;
        }

        throw new ArgumentOutOfRangeException(nameof(value), value, $"Invalid {typeof(TEnum).Name} value.");
    }

    ///sinh ra câu lệnh ràng buộc (Check Constraint) cho Database khi chạy Migration, ép Database không được nhận các chữ nằm ngoài danh sách Enum.
    public static string SqlInCheck<TEnum>(string columnName) where TEnum : struct, Enum
    {
        var values = string.Join(",", Enum.GetValues<TEnum>().Select(v => $"'{v.ToDbString()}'"));
        return $"\"{columnName}\" IN ({values})";
    }
}