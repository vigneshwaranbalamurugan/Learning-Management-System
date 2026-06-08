using System;
using System.Threading.Tasks;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services.Profile;
using LMSApi.DALLibrary.Repositories;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using Moq;
using NUnit.Framework;

namespace LMSApi.Tests.Services.Profile
{
    [TestFixture]
    public class ProfileServiceTests : BaseServiceTest
    {
        private Mock<IUploadService> _mockUploadService = null!;
        private IProfileService _profileService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockUploadService = new Mock<IUploadService>();
            
            var userProfileRepository = new UserProfileRepository(DbContext);
            var userRepository = new UserRepository(DbContext);

            _profileService = new ProfileService(
                userRepository,
                userProfileRepository,
                _mockUploadService.Object,
                Mapper
            );
        }

        [Test]
        public async Task GetProfileAsync_ExistingUser_ReturnsProfileResponse()
        {
            // Arrange
            var user = new Users
            {
                Email = "profile@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                IsEmailVerified = true,
                Role = new UserRoles { RoleName = "Learner", Description = "Desc" }
            };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

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
        }

        [Test]
        public void GetProfileAsync_NonExistingUser_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => _profileService.GetProfileAsync("nonexistent@example.com"));
        }

        [Test]
        public async Task UpdateProfileAsync_ValidRequest_UpdatesAndReturnsProfile()
        {
            // Arrange
            var user = new Users
            {
                Email = "update@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                IsEmailVerified = true,
                Role = new UserRoles { RoleName = "Learner", Description = "Desc" }
            };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

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
        }
    }
}
