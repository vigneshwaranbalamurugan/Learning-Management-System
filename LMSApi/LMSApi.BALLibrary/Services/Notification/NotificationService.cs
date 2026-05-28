using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using System.Linq;

namespace LMSApi.BALLibrary.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IEnumerable<INotificationHandler> _handlers;

        public NotificationService(IEnumerable<INotificationHandler> handlers)
        {
            _handlers = handlers;
        }

        public async Task Send(Message message)
        {
            var handler = _handlers.FirstOrDefault(item => item.CanHandle(message));

            if (handler is null)
            {
                throw new NotSupportedException($"No notification handler registered for message type '{message.MessageType}'.");
            }

            await handler.SendAsync(message);
        }
    }
}