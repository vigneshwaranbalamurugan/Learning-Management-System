namespace LMSApi.DALLibrary.Interfaces
{
    public interface ICurrentUserProvider
    {
        int? GetCurrentUserId();
    }
}
