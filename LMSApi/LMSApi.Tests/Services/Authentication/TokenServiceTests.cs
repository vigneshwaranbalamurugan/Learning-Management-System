using System;
using System.IdentityModel.Tokens.Jwt;
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

namespace LMSApi.Tests.Services
{
    [TestFixture]
    public class TokenServiceTests
    {
        private Mock<IConfiguration> _mockConfiguration = null!;
        private ITokenService _tokenService = null!;

        private const string ValidKey = "ThisIsAVerySecretKeyForTestingPurposes123!";
        private const string ValidIssuer = "TestIssuer";
        private const string ValidAudience = "TestAudience";

        [SetUp]
        public void SetUp()
        {
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns(ValidKey);
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns(ValidIssuer);
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns(ValidAudience);
            _mockConfiguration.Setup(c => c["Jwt:ExpiresMinutes"]).Returns("60");

            _tokenService = new TokenService(_mockConfiguration.Object);
        }

        // ─── GenerateToken ─────────────────────────────────────────────────────

        [Test]
        public void GenerateToken_ValidInputs_ReturnsTokenAndExpiry()
        {
            // Arrange
            int userId = 1;
            string email = "test@example.com";
            string role = "Learner";

            // Act
            var (token, expires) = _tokenService.GenerateToken(userId, email, role);

            // Assert
            Assert.That(token, Is.Not.Null.And.Not.Empty);
            Assert.That(expires, Is.GreaterThan(DateTime.UtcNow));
            Assert.That(expires, Is.LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(61)));
        }

        [Test]
        public void GenerateToken_TokenContainsExpectedClaims()
        {
            var (tokenStr, _) = _tokenService.GenerateToken(42, "claims@test.com", "Instructor");

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(tokenStr);

            Assert.That(jwt.Subject, Is.EqualTo("42"));
            Assert.That(jwt.Claims.Any(c => c.Value == "claims@test.com"), Is.True);
            Assert.That(jwt.Claims.Any(c => c.Value == "Instructor"), Is.True);
        }

        [Test]
        public void GenerateToken_NonIntegerExpiresMinutes_FallsBackTo60Minutes()
        {
            // "invalid" cannot be parsed → fallback to 60 minutes
            var config = new Mock<IConfiguration>();
            config.Setup(c => c["Jwt:Key"]).Returns(ValidKey);
            config.Setup(c => c["Jwt:Issuer"]).Returns(ValidIssuer);
            config.Setup(c => c["Jwt:Audience"]).Returns(ValidAudience);
            config.Setup(c => c["Jwt:ExpiresMinutes"]).Returns("invalid");

            var service = new TokenService(config.Object);
            var (_, expires) = service.GenerateToken(1, "e@e.com", "Learner");

            // Should expire roughly 60 minutes from now (within a 2-minute window)
            Assert.That(expires, Is.GreaterThan(DateTime.UtcNow.AddMinutes(58)));
            Assert.That(expires, Is.LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(61)));
        }

        [Test]
        public void GenerateToken_MissingKey_ThrowsInvalidOperationException()
        {
            // Arrange — Key is missing/null
            var invalidConfig = new Mock<IConfiguration>();
            // Key is missing
            var service = new TokenService(invalidConfig.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => service.GenerateToken(1, "test@example.com", "Learner"));
        }

        // ─── ValidateToken ─────────────────────────────────────────────────────

        [Test]
        public void ValidateToken_ValidToken_ReturnsTrue()
        {
            var (token, _) = _tokenService.GenerateToken(1, "valid@example.com", "Learner");
            var isValid = _tokenService.ValidateToken(token);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void ValidateToken_GarbageString_ReturnsFalse()
        {
            var isValid = _tokenService.ValidateToken("not.a.jwt");
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void ValidateToken_EmptyString_ReturnsFalse()
        {
            var isValid = _tokenService.ValidateToken(string.Empty);
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void ValidateToken_TokenSignedWithDifferentKey_ReturnsFalse()
        {
            // Generate token with a different key
            var altConfig = new Mock<IConfiguration>();
            altConfig.Setup(c => c["Jwt:Key"]).Returns("ACompletelyDifferentSecretKey9999!");
            altConfig.Setup(c => c["Jwt:Issuer"]).Returns(ValidIssuer);
            altConfig.Setup(c => c["Jwt:Audience"]).Returns(ValidAudience);
            altConfig.Setup(c => c["Jwt:ExpiresMinutes"]).Returns("60");

            var altService = new TokenService(altConfig.Object);
            var (token, _) = altService.GenerateToken(1, "alt@example.com", "Learner");

            // Validate with original service (different key)
            var isValid = _tokenService.ValidateToken(token);
            Assert.That(isValid, Is.False);
        }
    }
}
