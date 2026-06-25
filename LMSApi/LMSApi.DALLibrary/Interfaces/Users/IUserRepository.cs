using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IUserRepository : IRepository<int, Users>
    {
        Task<Users?> GetByEmailAsync(string email);
        Task<Users?> GetByRefreshTokenAsync(string refreshToken);
        Task<bool> IsEmailAlreadyRegisteredAsync(string email);
    }
}