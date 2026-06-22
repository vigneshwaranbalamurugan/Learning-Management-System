using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace LMSApi.API.Handlers
{
    /// <summary>
    /// Resolves the SignalR user identity from the JWT claim so that
    /// IHubContext.Clients.User(userId.ToString()) routes correctly.
    /// </summary>
    public class NotificationUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var user = connection.User;
            if (user is null) return null;

            var claim = user.FindFirst(ClaimTypes.NameIdentifier)
                        ?? user.FindFirst("sub");

            return claim?.Value;
        }
    }
}
