using System;
using System.Linq;
using System.Threading.Tasks;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;
using LMSApi.DALLibrary.Repositories;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;

namespace LMSApi.Tests.Services
{
    [TestFixture]
    public class UserNotificationsServiceTests : BaseServiceTest
    {
        private UserNotificationsRepository _repository = null!;
        private Mock<INotificationRealtimeService> _mockRealtimeService = null!;
        private Mock<ILogger<UserNotificationsService>> _mockLogger = null!;
        private UserNotificationsService _service = null!;
        private Users _testUser = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            // Create a test user
            _testUser = new Users
            {
                Email = "testuser@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                IsActive = true,
                IsEmailVerified = true,
                Role = new UserRoles { RoleName = "Student", Description = "Student Role" }
            };
            DbContext.Users.Add(_testUser);
            DbContext.SaveChanges();

            _repository = new UserNotificationsRepository(DbContext);
            _mockRealtimeService = new Mock<INotificationRealtimeService>();
            _mockLogger = new Mock<ILogger<UserNotificationsService>>();
            _service = new UserNotificationsService(_repository, Mapper, _mockRealtimeService.Object, _mockLogger.Object);
        }

        [Test]
        public async Task CreateAndSendNotificationAsync_SavesToDbAndPushesRealtime()
        {
            // Act
            await _service.CreateAndSendNotificationAsync(
                _testUser.Id,
                "Test Title",
                "Test Message",
                NotificationType.CourseEnrollment,
                "/courses/1"
            );

            // Assert DB persistence
            var notification = DbContext.Notifications.FirstOrDefault(n => n.UserId == _testUser.Id);
            Assert.That(notification, Is.Not.Null);
            Assert.That(notification.Title, Is.EqualTo("Test Title"));
            Assert.That(notification.Message, Is.EqualTo("Test Message"));
            Assert.That(notification.Type, Is.EqualTo(NotificationType.CourseEnrollment));
            Assert.That(notification.RedirectUrl, Is.EqualTo("/courses/1"));
            Assert.That(notification.IsRead, Is.False);
            Assert.That(notification.ReadAt, Is.Null);
            Assert.That((DateTime.UtcNow - notification.CreatedAt).TotalSeconds, Is.LessThan(5));

            // Assert Realtime broadcast called
            _mockRealtimeService.Verify(r => r.SendNotificationAsync(
                _testUser.Id,
                It.Is<ModelLibrary.DTOs.Notifications.NotificationResponse>(resp =>
                    resp.Title == "Test Title" &&
                    resp.Message == "Test Message" &&
                    resp.Type == "CourseEnrollment" &&
                    resp.RedirectUrl == "/courses/1" &&
                    resp.IsRead == false
                )
            ), Times.Once);

            // Assert logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating notification for user")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Real-time notification pushed successfully to user")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task GetUserNotificationsAsync_ReturnsMappedResponse()
        {
            // Arrange
            var n1 = new Notifications
            {
                UserId = _testUser.Id,
                Title = "T1",
                Message = "M1",
                Type = NotificationType.General,
                IsRead = false
            };
            var n2 = new Notifications
            {
                UserId = _testUser.Id,
                Title = "T2",
                Message = "M2",
                Type = NotificationType.QuizResult,
                IsRead = true,
                ReadAt = DateTime.UtcNow
            };
            DbContext.Notifications.AddRange(n1, n2);
            await DbContext.SaveChangesAsync();

            // Act
            var results = (await _service.GetUserNotificationsAsync(_testUser.Id)).ToList();

            // Assert
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results[0].Title, Is.EqualTo("T2"));
            Assert.That(results[1].Title, Is.EqualTo("T1"));
            Assert.That(results[1].Type, Is.EqualTo("General"));
            Assert.That(results[0].Type, Is.EqualTo("QuizResult"));
        }

        [Test]
        public async Task GetUnreadCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            var n1 = new Notifications { UserId = _testUser.Id, Title = "T1", Message = "M1", Type = NotificationType.General, IsRead = false };
            var n2 = new Notifications { UserId = _testUser.Id, Title = "T2", Message = "M2", Type = NotificationType.General, IsRead = true };
            var n3 = new Notifications { UserId = _testUser.Id, Title = "T3", Message = "M3", Type = NotificationType.General, IsRead = false };
            DbContext.Notifications.AddRange(n1, n2, n3);
            await DbContext.SaveChangesAsync();

            // Act
            var unreadCount = await _service.GetUnreadCountAsync(_testUser.Id);

            // Assert
            Assert.That(unreadCount, Is.EqualTo(2));
        }

        [Test]
        public async Task MarkAsReadAsync_UpdatesStatusAndTimestamp()
        {
            // Arrange
            var n = new Notifications { UserId = _testUser.Id, Title = "T", Message = "M", Type = NotificationType.General, IsRead = false };
            DbContext.Notifications.Add(n);
            await DbContext.SaveChangesAsync();

            // Act
            await _service.MarkAsReadAsync(_testUser.Id, n.Id);

            // Assert
            var updated = await DbContext.Notifications.FindAsync(n.Id);
            Assert.That(updated, Is.Not.Null);
            Assert.That(updated.IsRead, Is.True);
            Assert.That(updated.ReadAt, Is.Not.Null);
            Assert.That((DateTime.UtcNow - updated.ReadAt.Value).TotalSeconds, Is.LessThan(5));

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Marking notification") && v.ToString()!.Contains("as read for user")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("marked as read successfully for user")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public void MarkAsReadAsync_ForOtherUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var n = new Notifications { UserId = _testUser.Id, Title = "T", Message = "M", Type = NotificationType.General, IsRead = false };
            DbContext.Notifications.Add(n);
            DbContext.SaveChanges();

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _service.MarkAsReadAsync(_testUser.Id + 999, n.Id)
            );

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unauthorized attempt by user") && v.ToString()!.Contains("to mark notification")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task MarkAllAsReadAsync_UpdatesAllUnreadForUser()
        {
            // Arrange
            var n1 = new Notifications { UserId = _testUser.Id, Title = "T1", Message = "M1", Type = NotificationType.General, IsRead = false };
            var n2 = new Notifications { UserId = _testUser.Id, Title = "T2", Message = "M2", Type = NotificationType.General, IsRead = true };
            var n3 = new Notifications { UserId = _testUser.Id, Title = "T3", Message = "M3", Type = NotificationType.General, IsRead = false };
            var otherUser = new Users { Email = "other@test.com", PasswordHash = "h", PasswordSalt = "s", Role = _testUser.Role };
            DbContext.Users.Add(otherUser);
            DbContext.SaveChanges();
            var n4 = new Notifications { UserId = otherUser.Id, Title = "T4", Message = "M4", Type = NotificationType.General, IsRead = false };
            
            DbContext.Notifications.AddRange(n1, n2, n3, n4);
            await DbContext.SaveChangesAsync();

            // Act
            await _service.MarkAllAsReadAsync(_testUser.Id);

            // Assert
            var unreadCountUser = await DbContext.Notifications.CountAsync(n => n.UserId == _testUser.Id && !n.IsRead);
            var unreadCountOther = await DbContext.Notifications.CountAsync(n => n.UserId == otherUser.Id && !n.IsRead);

            Assert.That(unreadCountUser, Is.EqualTo(0));
            Assert.That(unreadCountOther, Is.EqualTo(1));

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Marking all notifications as read for user")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task DeleteNotificationAsync_RemovesFromDb()
        {
            // Arrange
            var n = new Notifications { UserId = _testUser.Id, Title = "T", Message = "M", Type = NotificationType.General, IsRead = false };
            DbContext.Notifications.Add(n);
            await DbContext.SaveChangesAsync();

            // Act
            await _service.DeleteNotificationAsync(_testUser.Id, n.Id);

            // Assert
            var deleted = await DbContext.Notifications.FindAsync(n.Id);
            Assert.That(deleted, Is.Null);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleting notification") && v.ToString()!.Contains("for user")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("deleted successfully for user")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public void DeleteNotificationAsync_ForOtherUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var n = new Notifications { UserId = _testUser.Id, Title = "T", Message = "M", Type = NotificationType.General, IsRead = false };
            DbContext.Notifications.Add(n);
            DbContext.SaveChanges();

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _service.DeleteNotificationAsync(_testUser.Id + 999, n.Id)
            );

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unauthorized attempt by user") && v.ToString()!.Contains("to delete notification")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
