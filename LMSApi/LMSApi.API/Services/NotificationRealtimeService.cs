using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs.Notifications;
using LMSApi.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace LMSApi.API.Services
{
    public class NotificationRealtimeService : INotificationRealtimeService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationRealtimeService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(int userId, NotificationResponse notification)
        {
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);
        }

        public async Task SendUnreadCountAsync(int userId, int unreadCount)
        {
            await _hubContext.Clients.User(userId.ToString()).SendAsync("UpdateUnreadCount", unreadCount);
        }
    }
}
