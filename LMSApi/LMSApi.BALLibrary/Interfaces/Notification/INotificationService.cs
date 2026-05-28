using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface INotificationService
    {
        Task Send(Message message);
    }
}