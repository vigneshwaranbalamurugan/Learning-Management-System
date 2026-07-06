using System;
using System.Threading.Tasks;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;
using LMSApi.DALLibrary.Repositories;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace LMSApi.Tests.Services
{
    [TestFixture]
    public class ProfileServiceTests : BaseServiceTest
    {
        private Mock<IUploadService> _uploadServiceMock = null!;
        private Mock<ICacheService> _cacheServiceMock = null!;
        private Mock<IConfiguration> _configMock = null!;
        private Mock<ILogger<ProfileService>> _loggerMock = null!;
        private IProfileService _profileService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _uploadServiceMock = new Mock<IUploadService>();
            _cacheServiceMock = new Mock<ICacheService>();

            // Pass-through: always call factory (simulates cache miss → go to DB)
            _cacheServiceMock
                .Setup(c => c.GetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<ProfileResponse>>>(),
                    It.IsAny<TimeSpan?>()))
                .Returns((string key, Func<Task<ProfileResponse>> factory, TimeSpan? expiry) => factory());
            _cacheServiceMock.Setup(c => c.InvalidateAsync(It.IsAny<string[]>())).Returns(Task.CompletedTask);
            _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            _configMock = new Mock<IConfiguration>();
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(s => s.Value).Returns("15");
            _configMock.Setup(c => c.GetSection(It.IsAny<string>())).Returns(configSection.Object);

            _loggerMock = new Mock<ILogger<ProfileService>>();

            var userProfileRepository = new UserProfileRepository(DbContext);
            var userRepository = new UserRepository(DbContext);

            _profileService = new ProfileService(
                userRepository,
                userProfileRepository,
                _uploadServiceMock.Object,
                Mapper,
                _cacheServiceMock.Object,
                _configMock.Object,
                _loggerMock.Object
            );
        }

        private async Task<Users> CreateUser(string email = "profile@example.com")
        {
            var user = new Users
            {
                Email = email,
                PasswordHash = "hash",
                PasswordSalt = "salt",
                IsEmailVerified = true,
                Role = new UserRoles { RoleName = "Learner", Description = "Desc" }
            };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();
            return user;
        }

        // ─── GetProfileAsync ───────────────────────────────────────────────────

        [Test]
        public async Task GetProfileAsync_ExistingUser_ReturnsProfileResponse()
        {
            // Arrange
            var user = await CreateUser();
            var profile = new UserProfiles
            {
                UserId = user.Id,
                FirstName = "John",
                LastName = "Doe",
                Bio = "Test Bio",
                Location = "Test Location",
                ProfilePictureUrl = "url",
                DateOfBirth = new DateOnly(2000, 1, 1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            DbContext.UserProfiles.Add(profile);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _profileService.GetProfileAsync(user.Email);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.FirstName, Is.EqualTo("John"));
            Assert.That(result.LastName, Is.EqualTo("Doe"));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieving profile for email")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public void GetProfileAsync_NonExistingUser_ThrowsNullReferenceException()
        {
            // The service does not guard against a null user from the repository;
            // it proceeds to access user.Id which throws NullReferenceException.
            Assert.ThrowsAsync<NullReferenceException>(() => _profileService.GetProfileAsync("nonexistent@example.com"));
        }

        [Test]
        public void GetProfileAsync_EmptyEmail_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _profileService.GetProfileAsync("  "));
        }

        [Test]
        public async Task GetProfileAsync_UserWithNoProfile_AutoCreatesDefaultProfile()
        {
            // Arrange — user with no UserProfiles entry
            var user = await CreateUser("noprofile@example.com");

            // Act
            var result = await _profileService.GetProfileAsync(user.Email);

            // Assert — a default profile is created automatically
            Assert.That(result, Is.Not.Null);
            // Default profile has empty strings
            Assert.That(result.FirstName, Is.EqualTo(string.Empty).Or.Null);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No profile found for user ID") && v.ToString()!.Contains("Creating default profile")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // ─── UpdateProfileAsync ────────────────────────────────────────────────

        [Test]
        public async Task UpdateProfileAsync_ValidRequest_UpdatesAndReturnsProfile()
        {
            // Arrange
            var user = await CreateUser("update@example.com");
            var profile = new UserProfiles
            {
                UserId = user.Id,
                FirstName = "Old",
                LastName = "Name",
                Bio = "Bio",
                ProfilePictureUrl = "url",
                DateOfBirth = new DateOnly(2000, 1, 1),
                Location = "Location",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            DbContext.UserProfiles.Add(profile);
            await DbContext.SaveChangesAsync();

            var request = new ProfileUpdateRequest
            {
                FirstName = "New",
                LastName = "Name",
                Bio = "Updated Bio",
                Location = "Updated Location"
            };

            // Act
            var result = await _profileService.UpdateProfileAsync(user.Email, request);

            // Assert
            Assert.That(result.FirstName, Is.EqualTo("New"));
            Assert.That(result.Bio, Is.EqualTo("Updated Bio"));

            var dbProfile = await DbContext.UserProfiles.FindAsync(profile.Id);
            Assert.That(dbProfile!.FirstName, Is.EqualTo("New"));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Updating profile for email")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Profile updated successfully for user ID")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public void UpdateProfileAsync_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                _profileService.UpdateProfileAsync("a@b.com", null!));
        }

        [Test]
        public void UpdateProfileAsync_EmptyEmail_ThrowsArgumentException()
        {
            var request = new ProfileUpdateRequest { FirstName = "John" };
            Assert.ThrowsAsync<ArgumentException>(() => _profileService.UpdateProfileAsync("  ", request));
        }

        [Test]
        public async Task UpdateProfileAsync_EmptyFirstName_ThrowsArgumentException()
        {
            var user = await CreateUser("firstname@example.com");
            var request = new ProfileUpdateRequest { FirstName = "  " }; // blank first name
            Assert.ThrowsAsync<ArgumentException>(() => _profileService.UpdateProfileAsync(user.Email, request));
        }

        // ─── UpdateProfileImageAsync ───────────────────────────────────────────

        [Test]
        public void UpdateProfileImageAsync_EmptyEmail_ThrowsArgumentException()
        {
            using var stream = new System.IO.MemoryStream(new byte[] { 1 });
            Assert.ThrowsAsync<ArgumentException>(() =>
                _profileService.UpdateProfileImageAsync("  ", stream, "pic.jpg", "image/jpeg"));
        }

        [Test]
        public void UpdateProfileImageAsync_NullStream_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                _profileService.UpdateProfileImageAsync("a@b.com", null!, "pic.jpg", "image/jpeg"));
        }

        [Test]
        public async Task UpdateProfileImageAsync_InvalidFileType_ThrowsInvalidOperationException()
        {
            var user = await CreateUser("imgtest@example.com");

            // Mock: file type not allowed
            _uploadServiceMock.Setup(s => s.IsAllowedProfileImage(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            using var stream = new System.IO.MemoryStream(new byte[] { 1, 2, 3 });
            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _profileService.UpdateProfileImageAsync(user.Email, stream, "malicious.exe", "application/octet-stream"));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to update profile image: disallowed file extension")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task UpdateProfileImageAsync_ValidJpeg_UploadsAndUpdatesProfile()
        {
            var user = await CreateUser("imgok@example.com");
            var profile = new UserProfiles
            {
                UserId = user.Id, FirstName = "John", LastName = "Doe",
                Bio = string.Empty, Location = string.Empty,
                ProfilePictureUrl = "old-url",
                DateOfBirth = new DateOnly(2000, 1, 1),
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            DbContext.UserProfiles.Add(profile);
            await DbContext.SaveChangesAsync();

            _uploadServiceMock.Setup(s => s.IsAllowedProfileImage(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _uploadServiceMock.Setup(s => s.UploadProfileImageAsync(It.IsAny<System.IO.Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://cdn.example.com/new-pic.jpg");

            using var stream = new System.IO.MemoryStream(new byte[] { 1, 2, 3 });
            var result = await _profileService.UpdateProfileImageAsync(user.Email, stream, "photo.jpg", "image/jpeg");

            Assert.That(result.ProfilePictureUrl, Is.EqualTo("https://cdn.example.com/new-pic.jpg"));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Profile image updated successfully for user ID")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
