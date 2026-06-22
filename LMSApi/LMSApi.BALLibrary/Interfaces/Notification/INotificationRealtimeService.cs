using LMSApi.ModelLibrary.DTOs.Notifications;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface INotificationRealtimeService
    {
        /// <summary>Pushes a new notification payload to the connected user.</summary>
        Task SendNotificationAsync(int userId, NotificationResponse notification);

        /// <summary>Pushes the updated unread count to the user so the badge stays in sync.</summary>
        Task SendUnreadCountAsync(int userId, int unreadCount);
    }
}
