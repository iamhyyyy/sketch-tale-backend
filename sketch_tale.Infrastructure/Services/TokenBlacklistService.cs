using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Caching.Memory;
using sketch_tale.Application.Interfaces.Services;

namespace sketch_tale.Infrastructure.Services;

public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(1);

    public TokenBlacklistService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task BlacklistTokenAsync(string token, TimeSpan? expiry = null)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.CompletedTask;

        TimeSpan ttl = expiry ?? CalculateRemainingTtl(token);
        if (ttl <= TimeSpan.Zero)
            ttl = TimeSpan.FromMinutes(5); // minimal safety margin

        _cache.Set(GetTokenKey(token), true, ttl);

        // Also extract and blacklist JTI if present
        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(token))
            {
                var jwt = handler.ReadJwtToken(token);
                if (!string.IsNullOrEmpty(jwt.Id))
                {
                    _cache.Set(GetJtiKey(jwt.Id), true, ttl);
                }
            }
        }
        catch
        {
            // Ignore parse errors on secondary jti extraction
        }

        return Task.CompletedTask;
    }

    public Task<bool> IsTokenBlacklistedAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult(false);

        bool isBlacklisted = _cache.TryGetValue(GetTokenKey(token), out _);
        return Task.FromResult(isBlacklisted);
    }

    public Task BlacklistJtiAsync(string jti, TimeSpan? expiry = null)
    {
        if (string.IsNullOrWhiteSpace(jti))
            return Task.CompletedTask;

        TimeSpan ttl = expiry ?? DefaultTtl;
        _cache.Set(GetJtiKey(jti), true, ttl);
        return Task.CompletedTask;
    }

    public Task<bool> IsJtiBlacklistedAsync(string jti)
    {
        if (string.IsNullOrWhiteSpace(jti))
            return Task.FromResult(false);

        bool isBlacklisted = _cache.TryGetValue(GetJtiKey(jti), out _);
        return Task.FromResult(isBlacklisted);
    }

    private static string GetTokenKey(string token) => $"blacklist:token:{token}";
    private static string GetJtiKey(string jti) => $"blacklist:jti:{jti}";

    private static TimeSpan CalculateRemainingTtl(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(token))
            {
                var jwt = handler.ReadJwtToken(token);
                var validTo = jwt.ValidTo;
                var remaining = validTo - DateTime.UtcNow;
                if (remaining > TimeSpan.Zero)
                {
                    return remaining.Add(TimeSpan.FromMinutes(1)); // add 1 min buffer
                }
            }
        }
        catch
        {
            // Fallback to default
        }

        return DefaultTtl;
    }
}
