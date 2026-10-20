using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using sketch_tale.Application.Interfaces.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace sketch_tale.Infrastructure.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<TokenBlacklistService> _logger;

        public TokenBlacklistService(IMemoryCache cache, ILogger<TokenBlacklistService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public Task BlacklistTokenAsync(string token)
        {
            var cleanToken = NormalizeToken(token);
            if (string.IsNullOrWhiteSpace(cleanToken))
            {
                return Task.CompletedTask;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(cleanToken))
                {
                    var jwtToken = handler.ReadJwtToken(cleanToken);
                    var expiryUtc = jwtToken.ValidTo;
                    var remaining = expiryUtc - DateTime.UtcNow;

                    // Thêm 1 phút buffer phòng trường hợp lệch đồng hồ (ClockSkew)
                    var cacheExpiry = remaining > TimeSpan.Zero
                        ? remaining.Add(TimeSpan.FromMinutes(1))
                        : TimeSpan.FromMinutes(5);

                    // 1. Blacklist chuỗi token đầy đủ
                    _cache.Set(GetTokenKey(cleanToken), true, cacheExpiry);

                    // 2. Blacklist mã định danh duy nhất của token (jti) nếu có
                    var jti = jwtToken.Claims.FirstOrDefault(c =>
                        c.Type == JwtRegisteredClaimNames.Jti || c.Type == "jti")?.Value;

                    if (!string.IsNullOrEmpty(jti))
                    {
                        _cache.Set(GetJtiKey(jti), true, cacheExpiry);
                    }

                    _logger.LogInformation("Token đã được đưa vào blacklist thành công. Hết hạn sau: {Duration}", cacheExpiry);
                }
                else
                {
                    // Nếu không parse được JWT thì lưu chuỗi token với thời gian mặc định 60 phút
                    _cache.Set(GetTokenKey(cleanToken), true, TimeSpan.FromMinutes(60));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi phân tích và blacklist token: {Message}", ex.Message);
                // Vẫn lưu raw token để bảo vệ
                _cache.Set(GetTokenKey(cleanToken), true, TimeSpan.FromMinutes(60));
            }

            return Task.CompletedTask;
        }

        public Task<bool> IsTokenBlacklistedAsync(string token)
        {
            var cleanToken = NormalizeToken(token);
            if (string.IsNullOrWhiteSpace(cleanToken))
            {
                return Task.FromResult(false);
            }

            // 1. Kiểm tra trực tiếp theo chuỗi raw token
            if (_cache.TryGetValue(GetTokenKey(cleanToken), out _))
            {
                return Task.FromResult(true);
            }

            // 2. Kiểm tra nếu input là jti hoặc trích xuất jti từ token
            if (_cache.TryGetValue(GetJtiKey(cleanToken), out _))
            {
                return Task.FromResult(true);
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(cleanToken))
                {
                    var jwtToken = handler.ReadJwtToken(cleanToken);
                    var jti = jwtToken.Claims.FirstOrDefault(c =>
                        c.Type == JwtRegisteredClaimNames.Jti || c.Type == "jti")?.Value;

                    if (!string.IsNullOrEmpty(jti) && _cache.TryGetValue(GetJtiKey(jti), out _))
                    {
                        return Task.FromResult(true);
                    }
                }
            }
            catch
            {
                // Bỏ qua lỗi parse, coi như không trích xuất được jti
            }

            return Task.FromResult(false);
        }

        private static string NormalizeToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return string.Empty;
            var trimmed = token.Trim();
            if (trimmed.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed.Substring(7).Trim();
            }
            return trimmed;
        }

        private static string GetTokenKey(string token) => $"bl_token:{token}";
        private static string GetJtiKey(string jti) => $"bl_jti:{jti}";
    }
}
