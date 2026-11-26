
using sketch_tale.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace sketch_tale.Application.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Lấy Id của user đang đăng nhập từ Claim
    public string? UserId => 
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub")
        ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("nameid");

    public string? Role =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role)
        ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("role");

    public bool IsInRole(string role)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return false;

        if (user.IsInRole(role)) return true;

        var userRole = Role;
        return string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase);
    }
}
