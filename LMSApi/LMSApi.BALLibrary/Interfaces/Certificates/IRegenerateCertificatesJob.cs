namespace LMSApi.BALLibrary.Interfaces
{
    public interface IRegenerateCertificatesJob
    {
        Task ExecuteAsync(int userId);
    }
}
