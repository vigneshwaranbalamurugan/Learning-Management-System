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
using LMSApi.BALLibrary.Services.Authentication;

namespace LMSApi.Tests.Services.Authentication
{
    [TestFixture]
    public class TokenServiceTests
    {
        private Mock<IConfiguration> _mockConfiguration = null!;
        private ITokenService _tokenService = null!;

        [SetUp]
        public void SetUp()
        {
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("ThisIsAVerySecretKeyForTestingPurposes123!");
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
            _mockConfiguration.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");

            _tokenService = new TokenService(_mockConfiguration.Object);
        }

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
        public void GenerateToken_MissingConfig_ThrowsException()
        {
            // Arrange
            var invalidConfig = new Mock<IConfiguration>();
            // Key is missing
            var service = new TokenService(invalidConfig.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => service.GenerateToken(1, "test@example.com", "Learner"));
        }
    }
}
