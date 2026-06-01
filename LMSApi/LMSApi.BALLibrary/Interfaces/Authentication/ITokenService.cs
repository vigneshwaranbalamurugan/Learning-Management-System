namespace LMSApi.BALLibrary.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateToken(int userId, string email, string role);
        bool ValidateToken(string token);
    }
}