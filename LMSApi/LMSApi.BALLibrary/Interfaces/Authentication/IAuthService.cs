using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> AuthenticateAsync(LoginRequest request);
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<VerifyEmailResponse> VerifyEmailAsync(VerifyEmailRequest request);
        Task<ResendVerificationResponse> ReRequestEmailVerificationAsync(ResendVerificationRequest request);
        Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request);
        Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task RevokeTokenAsync(string email, string? accessTokenJti, TimeSpan? accessTokenRemainingTtl);
    }
}