using LMSApi.ModelLibrary.DTOs.Notifications;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface INotificationRealtimeService
    {
        Task SendNotificationAsync(int userId, NotificationResponse notification);
    }
}
