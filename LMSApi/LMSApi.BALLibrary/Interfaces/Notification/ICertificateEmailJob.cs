using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ICertificateEmailJob
    {
        Task ExecuteAsync(int userId, string courseName, string certificateImageUrl, Guid certificateId);
    }
}
