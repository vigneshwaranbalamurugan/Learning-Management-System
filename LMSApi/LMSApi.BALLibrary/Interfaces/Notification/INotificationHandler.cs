using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface INotificationHandler
    {
        bool CanHandle(Message message);
        Task SendAsync(Message message);
    }
}