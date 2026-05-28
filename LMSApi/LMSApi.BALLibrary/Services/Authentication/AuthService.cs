using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.BALLibrary.Utils;
using Microsoft.Extensions.Configuration;


namespace LMSApi.BALLibrary.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, INotificationService notificationService, ITokenService tokenService, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _notificationService = notificationService;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task<LoginResponse> AuthenticateAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null) throw new KeyNotFoundException($"User with email {request.Email} not found");

            if (!PasswordHashing.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
                throw new UnauthorizedAccessException("Invalid credentials");

            if (!user.IsEmailVerified) throw new InvalidOperationException("Email not verified");

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var (token, expires) = _tokenService.GenerateToken(user.Email);
            return new LoginResponse { Email = request.Email, Token = token, ExpiresAt = expires, Message = "Authenticated" };
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            if(request.Role != RegistrationRole.Learner && request.Role != RegistrationRole.Instructor)
                throw new InvalidOperationException("Invalid registration role.Only Learner and Instructor roles are allowed");

            if (await _userRepository.IsEmailAlreadyRegisteredAsync(request.Email))
                throw new InvalidOperationException($"Email {request.Email} is already registered");
            var (passwordHash, passwordSalt) = PasswordHashing.HashPassword(request.Password);
            var roleId = request.Role switch
            {
                RegistrationRole.Learner => 1,
                RegistrationRole.Instructor => 2,
                _ => throw new InvalidOperationException("Invalid registration role")
            };

            var user = new Users
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                IsActive = true,
                CurrentTokenType = TokenType.EmailVerification,
                IsEmailVerified = false,
                RoleId = roleId,
                VerificationToken = Guid.NewGuid().ToString(),
                VerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            var relative = $"/api/auth/verify?email={Uri.EscapeDataString(request.Email)}&token={user.VerificationToken}";
            var baseUrl = _configuration["App:BaseUrl"] ?? string.Empty;
            var link = string.IsNullOrEmpty(baseUrl) ? relative : (baseUrl.TrimEnd('/') + relative);

            var html = EmailTemplate.GetVerificationTemplate(request.Email, link);
            Message msg = new EmailMessage(request.Email, "Please verify your email", html) { IsHtml = true };
            await _notificationService.Send(msg);

            return new RegisterResponse { Email = request.Email, Message = "Registration successful. Verification email sent. Please check your inbox and verify your email." };
        }

        public async Task<VerifyEmailResponse> VerifyEmailAsync(VerifyEmailRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null) throw new KeyNotFoundException($"User with email {request.Email} not found");
            if (user.IsEmailVerified) throw new InvalidOperationException($"Email {request.Email} is already verified");
            if(user.CurrentTokenType != TokenType.EmailVerification) throw new InvalidOperationException("No email verification in progress");
            if (user.VerificationToken != request.Token) throw new UnauthorizedAccessException("Invalid verification token");
            if (user.VerificationTokenExpiry == null || user.VerificationTokenExpiry < DateTime.UtcNow) throw new InvalidOperationException("Verification token expired");

            user.IsEmailVerified = true;
            user.VerificationToken = null;
            user.VerificationTokenExpiry = null;
            user.CurrentTokenType = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return new VerifyEmailResponse { IsVerified = true, Email = request.Email, Message = "Email verified successfully" };
        }

        public async Task<ResendVerificationResponse> ReRequestEmailVerificationAsync(ResendVerificationRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null) throw new KeyNotFoundException($"User with email {request.Email} not found");
            if (user.IsEmailVerified) throw new InvalidOperationException($"Email {request.Email} is already verified");
            if(user.CurrentTokenType != TokenType.EmailVerification) throw new InvalidOperationException("No email verification in progress");

            user.VerificationToken = Guid.NewGuid().ToString();
            user.VerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
            user.CurrentTokenType = TokenType.EmailVerification;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            var relative = $"/api/auth/verify?email={Uri.EscapeDataString(request.Email)}&token={user.VerificationToken}";
            var baseUrl = _configuration["App:BaseUrl"] ?? string.Empty;
            var link = string.IsNullOrEmpty(baseUrl) ? relative : (baseUrl.TrimEnd('/') + relative);
            var html = EmailTemplate.GetVerificationTemplate(request.Email, link);
            Message msg = new EmailMessage(request.Email, "Your verification link", html) { IsHtml = true };
            await _notificationService.Send(msg);

            return new ResendVerificationResponse { IsSent = true, Email = request.Email, Message = "Verification email sent to your inbox." };
        }
    }
}
