namespace LMSApi.BALLibrary.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateToken(string email);
        bool ValidateToken(string token);
    }
}