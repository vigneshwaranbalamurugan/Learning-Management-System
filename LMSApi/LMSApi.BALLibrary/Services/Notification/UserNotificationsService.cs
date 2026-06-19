using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs.Notifications;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services
{
    public class UserNotificationsService : IUserNotificationsService
    {
        private readonly IUserNotificationsRepository _notificationsRepository;
        private readonly IMapper _mapper;
        private readonly INotificationRealtimeService _realtimeService;
        private readonly ILogger<UserNotificationsService>? _logger;

        public UserNotificationsService(
            IUserNotificationsRepository notificationsRepository,
            IMapper mapper,
            INotificationRealtimeService realtimeService,
            ILogger<UserNotificationsService>? logger = null)
        {
            _notificationsRepository = notificationsRepository;
            _mapper = mapper;
            _realtimeService = realtimeService;
            _logger = logger;
        }

        public async Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(int userId)
        {
            var notifications = await _notificationsRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<NotificationResponse>>(notifications);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _notificationsRepository.GetUnreadCountByUserIdAsync(userId);
        }

        public async Task MarkAsReadAsync(int userId, int notificationId)
        {
            _logger?.LogInformation("Marking notification {NotificationId} as read for user {UserId}", notificationId, userId);

            var notification = await _notificationsRepository.GetByIdAsync(notificationId);
            if (notification.UserId != userId)
            {
                _logger?.LogWarning("Unauthorized attempt by user {UserId} to mark notification {NotificationId} as read.", userId, notificationId);
                throw new UnauthorizedAccessException("You do not have permission to access this notification.");
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _notificationsRepository.UpdateAsync(notification);
                _logger?.LogInformation("Notification {NotificationId} marked as read successfully for user {UserId}", notificationId, userId);
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            _logger?.LogInformation("Marking all notifications as read for user {UserId}", userId);
            await _notificationsRepository.MarkAllAsReadAsync(userId);
        }

        public async Task DeleteNotificationAsync(int userId, int notificationId)
        {
            _logger?.LogInformation("Deleting notification {NotificationId} for user {UserId}", notificationId, userId);

            var notification = await _notificationsRepository.GetByIdAsync(notificationId);
            if (notification.UserId != userId)
            {
                _logger?.LogWarning("Unauthorized attempt by user {UserId} to delete notification {NotificationId}.", userId, notificationId);
                throw new UnauthorizedAccessException("You do not have permission to delete this notification.");
            }

            await _notificationsRepository.DeleteAsync(notificationId);
            _logger?.LogInformation("Notification {NotificationId} deleted successfully for user {UserId}", notificationId, userId);
        }

        public async Task CreateAndSendNotificationAsync(int userId, string title, string message, NotificationType type, string? redirectUrl = null)
        {
            _logger?.LogInformation("Creating notification for user {UserId} (Type: {Type}, Title: {Title})", userId, type, title);

            var notification = new Notifications
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                RedirectUrl = redirectUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationsRepository.AddAsync(notification);

            var response = _mapper.Map<NotificationResponse>(notification);
            await _realtimeService.SendNotificationAsync(userId, response);
            _logger?.LogInformation("Real-time notification pushed successfully to user {UserId}.", userId);
        }
    }
}
