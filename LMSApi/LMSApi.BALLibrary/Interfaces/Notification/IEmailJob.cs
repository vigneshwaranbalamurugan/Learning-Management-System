using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IEmailJob
    {
        Task ExecuteAsync(EmailMessage message);
    }
}
