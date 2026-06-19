using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace LMSApi.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}
