using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LMSApi.BALLibrary.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LMSApi.BALLibrary.Services.Authentication
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string Token, DateTime ExpiresAt) GenerateToken(int userId, string email, string role)
        {
            try{
                var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured");
                var issuer = _configuration["Jwt:Issuer"] ?? "LMSApi";
                var audience = _configuration["Jwt:Audience"] ?? "LMSApiUsers";
                var expiresMinutesStr = _configuration["Jwt:ExpiresMinutes"] ?? "60";
                if (!int.TryParse(expiresMinutesStr, out var expiresMinutes)) expiresMinutes = 60;

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, role)
                };

                var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: expires,
                    signingCredentials: credentials
                );

                var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
                return (tokenStr, expires);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to generate token", ex);
            }
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured");
                var issuer = _configuration["Jwt:Issuer"] ?? "LMSApi";
                var audience = _configuration["Jwt:Audience"] ?? "LMSApiUsers";

                var tokenHandler = new JwtSecurityTokenHandler();
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateLifetime = true
                };

                tokenHandler.ValidateToken(token, parameters, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
