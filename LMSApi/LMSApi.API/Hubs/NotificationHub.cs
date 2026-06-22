using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LMSApi.API.Hubs
{
    /// <summary>
    /// Real-time SignalR hub for user notifications.
    ///
    /// Client workflow:
    ///   1. Connect to /hubs/notification (cookie auth via withCredentials).
    ///   2. Listen for "ReceiveNotification" — fires when a new notification is created for this user.
    ///   3. Listen for "UpdateUnreadCount" — fires after any read/create event to keep the badge in sync.
    ///   4. Optionally call MarkAsRead(id) over the WebSocket to acknowledge a notification.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly IUserNotificationsService _notificationsService;
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(
            IUserNotificationsService notificationsService,
            ILogger<NotificationHub> logger)
        {
            _notificationsService = notificationsService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User!.GetUserId();
            _logger.LogInformation("User {UserId} connected to NotificationHub (ConnectionId: {ConnectionId})", userId, Context.ConnectionId);

            // Push the current unread count as soon as the client connects
            try
            {
                var unreadCount = await _notificationsService.GetUnreadCountAsync(userId);
                await Clients.Caller.SendAsync("UpdateUnreadCount", unreadCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send initial unread count to User {UserId}", userId);
            }

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User!.GetUserId();

            if (exception is null)
                _logger.LogInformation("User {UserId} disconnected from NotificationHub (ConnectionId: {ConnectionId})", userId, Context.ConnectionId);
            else
                _logger.LogWarning(exception, "User {UserId} disconnected from NotificationHub with error (ConnectionId: {ConnectionId})", userId, Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Called by the client to mark a single notification as read over the WebSocket.
        /// Emits an updated "UpdateUnreadCount" back to the caller.
        /// </summary>
        public async Task MarkAsRead(int notificationId)
        {
            var userId = Context.User!.GetUserId();
            try
            {
                await _notificationsService.MarkAsReadAsync(userId, notificationId);
                var unreadCount = await _notificationsService.GetUnreadCountAsync(userId);
                await Clients.Caller.SendAsync("UpdateUnreadCount", unreadCount);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized MarkAsRead attempt by User {UserId} for notification {NotificationId}", userId, notificationId);
                await Clients.Caller.SendAsync("Error", new { Message = "You do not have permission to access this notification." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read for User {UserId}", notificationId, userId);
                await Clients.Caller.SendAsync("Error", new { Message = "Failed to mark notification as read." });
            }
        }

        /// <summary>
        /// Called by the client to mark all notifications as read over the WebSocket.
        /// </summary>
        public async Task MarkAllAsRead()
        {
            var userId = Context.User!.GetUserId();
            try
            {
                await _notificationsService.MarkAllAsReadAsync(userId);
                await Clients.Caller.SendAsync("UpdateUnreadCount", 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for User {UserId}", userId);
                await Clients.Caller.SendAsync("Error", new { Message = "Failed to mark all notifications as read." });
            }
        }
    }
}
