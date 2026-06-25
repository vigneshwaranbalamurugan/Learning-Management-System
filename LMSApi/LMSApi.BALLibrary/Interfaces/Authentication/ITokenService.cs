using System.Security.Claims;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateToken(int userId, string email, string role, int? expiresMinutesOverride = null);
        bool ValidateToken(string token);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}