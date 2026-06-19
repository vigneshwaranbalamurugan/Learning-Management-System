using LMSApi.BALLibrary.Interfaces;
using LMSApi.API.Extensions;
using LMSApi.ModelLibrary.DTOs.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;

namespace LMSApi.API.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [EnableRateLimiting("NotificationApis")]
    public class NotificationsController : ControllerBase
    {
        private readonly IUserNotificationsService _notificationsService;

        public NotificationsController(IUserNotificationsService notificationsService)
        {
            _notificationsService = notificationsService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetNotifications()
        {
            var userId = User.GetUserId();
            var result = await _notificationsService.GetUserNotificationsAsync(userId);
            return Ok(result);
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var userId = User.GetUserId();
            var result = await _notificationsService.GetUnreadCountAsync(userId);
            return Ok(result);
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.GetUserId();
            await _notificationsService.MarkAsReadAsync(userId, id);
            return NoContent();
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.GetUserId();
            await _notificationsService.MarkAllAsReadAsync(userId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userId = User.GetUserId();
            await _notificationsService.DeleteNotificationAsync(userId, id);
            return NoContent();
        }
    }
}
