

namespace sketch_tale.Application.Interfaces.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Role { get; }
    bool IsInRole(string role);
}
