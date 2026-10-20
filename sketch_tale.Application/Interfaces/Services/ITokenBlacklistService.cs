namespace sketch_tale.Application.Interfaces.Services;

public interface ITokenBlacklistService
{
    Task BlacklistTokenAsync(string token, TimeSpan? expiry = null);
    Task<bool> IsTokenBlacklistedAsync(string token);
    Task BlacklistJtiAsync(string jti, TimeSpan? expiry = null);
    Task<bool> IsJtiBlacklistedAsync(string jti);
}
