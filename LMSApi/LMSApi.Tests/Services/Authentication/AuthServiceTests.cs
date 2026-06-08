using System;
using System.Threading.Tasks;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;
using LMSApi.DALLibrary.Repositories;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using LMSApi.BALLibrary.Utils;

namespace LMSApi.Tests.Services.Authentication
{
    [TestFixture]
    public class AuthServiceTests : BaseServiceTest
    {
        private Mock<INotificationService> _mockNotificationService = null!;
        private Mock<ITokenService> _mockTokenService = null!;
        private Mock<IConfiguration> _mockConfiguration = null!;
        private IAuthService _authService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockNotificationService = new Mock<INotificationService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(c => c["App:BaseUrl"]).Returns("https://test.com");
            _mockConfiguration.Setup(c => c["App:BasePath"]).Returns("/api");

            var userRepository = new UserRepository(DbContext);

            _authService = new AuthService(
                userRepository,
                _mockNotificationService.Object,
                _mockTokenService.Object,
                _mockConfiguration.Object
            );
        }

        [Test]
        public async Task AuthenticateAsync_ValidCredentials_ReturnsLoginResponse()
        {
            // Arrange
            var (hash, salt) = PasswordHashing.HashPassword("Password123!");
            var user = new Users
            {
                Email = "test@example.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                IsEmailVerified = true,
                Role = new UserRoles { RoleName = "Learner", Description = "Desc" }
            };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(("mock_token", DateTime.UtcNow.AddHours(1)));

            var request = new LoginRequest { Email = "test@example.com", Password = "Password123!" };

            // Act
            var response = await _authService.AuthenticateAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Token, Is.EqualTo("mock_token"));
        }

        [Test]
        public void AuthenticateAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var (hash, salt) = PasswordHashing.HashPassword("Password123!");
            var user = new Users
            {
                Email = "test@example.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                IsEmailVerified = true,
                Role = new UserRoles { RoleName = "Learner", Description = "Desc" }
            };
            DbContext.Users.Add(user);
            DbContext.SaveChanges();

            var request = new LoginRequest { Email = "test@example.com", Password = "WrongPassword!" };

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.AuthenticateAsync(request));
        }

        [Test]
        public async Task RegisterAsync_ValidRequest_SendsVerificationEmailAndReturnsResponse()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "newuser@example.com",
                Password = "Password123!",
                Role = RegistrationRole.Learner
            };

            // Act
            var response = await _authService.RegisterAsync(request);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Message, Does.Contain("Verification email sent"));
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Once);
        }

        [Test]
        public void RegisterAsync_ExistingEmail_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = new Users
            {
                Email = "existing@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                Role = new UserRoles { RoleName = "Learner", Description = "Desc" }
            };
            DbContext.Users.Add(user);
            DbContext.SaveChanges();

            var request = new RegisterRequest
            {
                Email = "existing@example.com",
                Password = "Password123!",
                Role = RegistrationRole.Learner
            };

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(request));
        }

        [Test]
        public async Task VerifyEmailAsync_ValidToken_VerifiesEmailSuccessfully()
        {
            // Arrange
            var user = new Users
            {
                Email = "verify@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                IsEmailVerified = false,
                CurrentTokenType = TokenType.EmailVerification,
                VerificationToken = "valid_token",
                VerificationTokenExpiry = DateTime.UtcNow.AddHours(1),
                Role = new UserRoles { RoleName = "Learner", Description = "Desc" }
            };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var request = new VerifyEmailRequest { Email = "verify@example.com", Token = "valid_token" };

            // Act
            var response = await _authService.VerifyEmailAsync(request);

            // Assert
            Assert.That(response.IsVerified, Is.True);
        }

        [Test]
        public void VerifyEmailAsync_ExpiredToken_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = new Users
            {
                Email = "expired@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                IsEmailVerified = false,
                CurrentTokenType = TokenType.EmailVerification,
                VerificationToken = "expired_token",
                VerificationTokenExpiry = DateTime.UtcNow.AddHours(-1),
                Role = new UserRoles { RoleName = "Learner", Description = "Desc" }
            };
            DbContext.Users.Add(user);
            DbContext.SaveChanges();

            var request = new VerifyEmailRequest { Email = "expired@example.com", Token = "expired_token" };

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => _authService.VerifyEmailAsync(request));
        }

        [Test]
        public async Task ReRequestEmailVerificationAsync_ValidRequest_SendsNewEmail()
        {
            // Arrange
            var user = new Users
            {
                Email = "resend@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                IsEmailVerified = false,
                CurrentTokenType = TokenType.EmailVerification,
                VerificationToken = "old_token",
                VerificationTokenExpiry = DateTime.UtcNow.AddHours(-1),
                Role = new UserRoles { RoleName = "Learner", Description = "Desc" }
            };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var request = new ResendVerificationRequest { Email = "resend@example.com" };

            // Act
            var response = await _authService.ReRequestEmailVerificationAsync(request);

            // Assert
            Assert.That(response.IsSent, Is.True);
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Once);
        }
    }
}
