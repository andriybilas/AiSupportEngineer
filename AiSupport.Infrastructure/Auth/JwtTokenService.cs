using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AiSupport.Infrastructure.Auth
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public JwtTokenResult CreateToken(Guid userId, string userName)
        {
            var issuer = _configuration["Jwt:Issuer"] ?? "AiSupportEngineer";
            var audience = _configuration["Jwt:Audience"] ?? "AiSupportEngineer";
            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is not configured.");

            var expiresMinutesText = _configuration["Jwt:ExpiresMinutes"];
            var expiresMinutes = 60;

            if (!string.IsNullOrWhiteSpace(expiresMinutesText)
                && int.TryParse(expiresMinutesText, out var parsedMinutes)
                && parsedMinutes > 0)
            {
                expiresMinutes = parsedMinutes;
            }

            var expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, userName),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new JwtTokenResult
            {
                Token = tokenString,
                ExpiresAt = expiresAt
            };
        }
    }
}
