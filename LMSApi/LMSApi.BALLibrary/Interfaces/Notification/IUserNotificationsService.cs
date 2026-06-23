using LMSApi.ModelLibrary.DTOs.Notifications;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IUserNotificationsService
    {
        Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(int userId, int skip, int take);
        Task<int> GetUnreadCountAsync(int userId);
        Task MarkAsReadAsync(int userId, int notificationId);
        Task MarkAllAsReadAsync(int userId);
        Task DeleteNotificationAsync(int userId, int notificationId);
        Task CreateAndSendNotificationAsync(int userId, string title, string message, NotificationType type, string? redirectUrl = null);
    }
}
