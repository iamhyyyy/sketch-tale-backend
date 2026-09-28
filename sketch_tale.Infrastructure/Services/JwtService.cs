using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            // ĐOẠN MỚI ĐÃ SỬA CHO KHỚP VỚI appsettings.json
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret is missing.");
            int expiryMinutes = int.TryParse(jwtSettings["ExpiryMinutes"], out var minutes) ? minutes : 60;

            var key = Encoding.UTF8.GetBytes(secretKey);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim("role", "User")
        };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
