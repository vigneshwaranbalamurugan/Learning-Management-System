using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.BALLibrary.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace LMSApi.BALLibrary.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService>? _logger;

        public AuthService(
            IUserRepository userRepository, 
            INotificationService notificationService, 
            ITokenService tokenService, 
            IConfiguration configuration,
            ILogger<AuthService>? logger = null)
        {
            _userRepository = userRepository;
            _notificationService = notificationService;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
            PasswordHashing.Initialize(configuration);
        }

        public async Task<LoginResponse> AuthenticateAsync(LoginRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Email cannot be null or empty.", nameof(request.Email));
            if (string.IsNullOrWhiteSpace(request.Password)) throw new ArgumentException("Password cannot be null or empty.", nameof(request.Password));

            _logger?.LogInformation("Attempting to authenticate user: {Email}", request.Email);

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                _logger?.LogWarning("Authentication failed: User {Email} not found.", request.Email);
                throw new UnauthorizedAccessException($"Invalid credentials");
            }

            if (!PasswordHashing.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                _logger?.LogWarning("Authentication failed: Incorrect password for user {Email}.", request.Email);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            if (!user.IsEmailVerified)
            {
                _logger?.LogWarning("Authentication failed: User {Email} has not verified their email.", request.Email);
                throw new InvalidOperationException("Email not verified");
            }

            if (user.Role == null)
            {
                _logger?.LogError("Authentication failed: User role navigation property is null for user {Email}.", request.Email);
                throw new InvalidOperationException("User role is not configured.");
            }

            var (token, expires) = _tokenService.GenerateToken(user.Id, user.Email, user.Role.RoleName);
            
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenDaysStr = _configuration["Jwt:RefreshTokenExpiresDays"] ?? "7";
            if (!int.TryParse(refreshTokenDaysStr, out var refreshDays)) refreshDays = 7;

            user.LastLoginAt = DateTime.UtcNow;
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshDays);
            await _userRepository.UpdateAsync(user);

            _logger?.LogInformation("User {Email} authenticated successfully. Token expires at: {ExpiresAt}", request.Email, expires);
            return new LoginResponse { Email = request.Email, Token = token, ExpiresAt = expires, RefreshToken = refreshToken, Message = "Authenticated" };
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Email cannot be null or empty.", nameof(request.Email));
            if (string.IsNullOrWhiteSpace(request.Password)) throw new ArgumentException("Password cannot be null or empty.", nameof(request.Password));

            _logger?.LogInformation("Registering new user with email: {Email}, role: {Role}", request.Email, request.Role);

            if(request.Role != RegistrationRole.Learner && request.Role != RegistrationRole.Instructor)
            {
                _logger?.LogWarning("Registration failed: Invalid role {Role} requested for email {Email}.", request.Role, request.Email);
                throw new InvalidOperationException("Invalid registration role.Only Learner and Instructor roles are allowed");
            }

            if (await _userRepository.IsEmailAlreadyRegisteredAsync(request.Email))
            {
                _logger?.LogWarning("Registration failed: Email {Email} is already registered.", request.Email);
                throw new InvalidOperationException($"Email {request.Email} is already registered");
            }
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
            };

            await _userRepository.AddAsync(user);
            _logger?.LogInformation("User entity created for {Email} with ID: {UserId}.", user.Email, user.Id);

            var relative = $"/auth/verify?email={Uri.EscapeDataString(request.Email)}&token={user.VerificationToken}";
            var baseUrl = _configuration["App:BaseUrl"] ?? string.Empty;
            var basePath = _configuration["App:BasePath"] ?? string.Empty;
            var link = string.IsNullOrEmpty(baseUrl) ? relative : (baseUrl.TrimEnd('/') + basePath + relative);

            var html = EmailTemplate.GetVerificationTemplate(request.Email, link);
            Message msg = new EmailMessage(request.Email, "Please verify your email", html) { IsHtml = true };
            await _notificationService.Send(msg);
            _logger?.LogInformation("Verification email sent to {Email}.", request.Email);

            return new RegisterResponse { Email = request.Email, Message = "Registration successful. Verification email sent. Please check your inbox and verify your email." };
        }

        public async Task<VerifyEmailResponse> VerifyEmailAsync(VerifyEmailRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Email cannot be null or empty.", nameof(request.Email));
            if (string.IsNullOrWhiteSpace(request.Token)) throw new ArgumentException("Verification token cannot be null or empty.", nameof(request.Token));

            _logger?.LogInformation("Attempting to verify email for: {Email}", request.Email);

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                _logger?.LogWarning("Email verification failed: User {Email} not found.", request.Email);
                throw new UnauthorizedAccessException("Invalid verification token or email");
            }
            if (user.IsEmailVerified)
            {
                _logger?.LogWarning("Email verification failed: User {Email} is already verified.", request.Email);
                throw new UnauthorizedAccessException("Invalid verification token or email");
            }
            if(user.CurrentTokenType != TokenType.EmailVerification)
            {
                _logger?.LogWarning("Email verification failed: Incorrect token type for user {Email}.", request.Email);
                throw new InvalidOperationException("Invalid verification token or email");
            }
            if (user.VerificationToken != request.Token)
            {
                _logger?.LogWarning("Email verification failed: Token mismatch for user {Email}.", request.Email);
                throw new UnauthorizedAccessException("Invalid verification token or email");
            }
            if (user.VerificationTokenExpiry == null || user.VerificationTokenExpiry < DateTime.UtcNow)
            {
                _logger?.LogWarning("Email verification failed: Verification token expired for user {Email}.", request.Email);
                throw new InvalidOperationException("Invalid verification token or email");
            }

            user.IsEmailVerified = true;
            user.VerificationToken = null;
            user.VerificationTokenExpiry = null;
            user.CurrentTokenType = null;

            await _userRepository.UpdateAsync(user);
            _logger?.LogInformation("Email verified successfully for user: {Email}", request.Email);

            var html = EmailTemplate.GetWelcomeTemplate(user.Email, user.Email, user.Role?.RoleName ?? "Learner");
            Message msg = new EmailMessage(user.Email, "Welcome to LMS!", html) { IsHtml = true };
            await _notificationService.Send(msg);

            return new VerifyEmailResponse { IsVerified = true, Email = request.Email, Message = "Email verified successfully" };
        }

        public async Task<ResendVerificationResponse> ReRequestEmailVerificationAsync(ResendVerificationRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Email cannot be null or empty.", nameof(request.Email));

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null) throw new UnauthorizedAccessException("Invalid verification token or email");
            if (user.IsEmailVerified) throw new InvalidOperationException("Invalid verification token or email");
            if(user.CurrentTokenType != TokenType.EmailVerification) throw new InvalidOperationException("Invalid verification token or email");

            user.VerificationToken = Guid.NewGuid().ToString();
            user.VerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
            user.CurrentTokenType = TokenType.EmailVerification;

            await _userRepository.UpdateAsync(user);

            var relative = $"/auth/verify?email={Uri.EscapeDataString(request.Email)}&token={user.VerificationToken}";
            var baseUrl = _configuration["App:BaseUrl"] ?? string.Empty;
            var basePath = _configuration["App:BasePath"] ?? string.Empty;
            var link = string.IsNullOrEmpty(baseUrl) ? relative : (baseUrl.TrimEnd('/') + basePath + relative);
            var html = EmailTemplate.GetVerificationTemplate(request.Email, link);
            Message msg = new EmailMessage(request.Email, "Your verification link", html) { IsHtml = true };
            await _notificationService.Send(msg);

            return new ResendVerificationResponse { IsSent = true, Email = request.Email, Message = "Verification email sent to your inbox." };
        }
        public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Email cannot be null or empty.", nameof(request.Email));

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null) 
            {
                // To prevent email enumeration, return a success message even if the user is not found.
                return new ForgotPasswordResponse { Email = request.Email, Message = "If an account with that email exists, a password reset link has been sent." };
            }

            user.VerificationToken = Guid.NewGuid().ToString();
            user.VerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
            user.CurrentTokenType = TokenType.PasswordReset;

            await _userRepository.UpdateAsync(user);

            var relative = $"/auth/reset-password?email={Uri.EscapeDataString(request.Email)}&token={user.VerificationToken}";
            var baseUrl = _configuration["App:BaseUrl"] ?? string.Empty;
            var basePath = _configuration["App:BasePath"] ?? string.Empty;
            var link = string.IsNullOrEmpty(baseUrl) ? relative : (baseUrl.TrimEnd('/') + basePath + relative);
            var html = EmailTemplate.GetPasswordResetTemplate(request.Email, link);
            
            Message msg = new EmailMessage(request.Email, "Reset Your Password", html) { IsHtml = true };
            await _notificationService.Send(msg);

            return new ForgotPasswordResponse { Email = request.Email, Message = "If an account with that email exists, a password reset link has been sent." };
        }

        public async Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Email cannot be null or empty.", nameof(request.Email));
            if (string.IsNullOrWhiteSpace(request.Token)) throw new ArgumentException("Token cannot be null or empty.", nameof(request.Token));
            if (string.IsNullOrWhiteSpace(request.NewPassword)) throw new ArgumentException("New Password cannot be null or empty.", nameof(request.NewPassword));

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null) throw new KeyNotFoundException("Invalid password reset token or email");

            if (user.CurrentTokenType != TokenType.PasswordReset) throw new InvalidOperationException("Invalid password reset token or email");
            if (user.VerificationToken != request.Token) throw new UnauthorizedAccessException("Invalid password reset token or email");
            if (user.VerificationTokenExpiry == null || user.VerificationTokenExpiry < DateTime.UtcNow) throw new InvalidOperationException("Invalid password reset token or email");

            var (passwordHash, passwordSalt) = PasswordHashing.HashPassword(request.NewPassword);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            user.VerificationToken = null;
            user.VerificationTokenExpiry = null;
            user.CurrentTokenType = null;

            await _userRepository.UpdateAsync(user);

            return new ResetPasswordResponse { Email = request.Email, Message = "Password has been successfully reset." };
        }

        public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.AccessToken)) throw new ArgumentException("Access token is required", nameof(request.AccessToken));
            if (string.IsNullOrWhiteSpace(request.RefreshToken)) throw new ArgumentException("Refresh token is required", nameof(request.RefreshToken));

            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
            {
                throw new UnauthorizedAccessException("Invalid access token or refresh token");
            }

            var email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                throw new UnauthorizedAccessException("Invalid access token or refresh token");
            }

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Invalid access token or refresh token");
            }

            var (newAccessToken, expiresAt) = _tokenService.GenerateToken(user.Id, user.Email, user.Role.RoleName);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            var refreshTokenDaysStr = _configuration["Jwt:RefreshTokenExpiresDays"] ?? "7";
            if (!int.TryParse(refreshTokenDaysStr, out var refreshDays)) refreshDays = 7;

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshDays);
            await _userRepository.UpdateAsync(user);

            return new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = expiresAt
            };
        }

        public async Task RevokeTokenAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required", nameof(email));

            var user = await _userRepository.GetByEmailAsync(email);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userRepository.UpdateAsync(user);
            }
        }
    }
}
