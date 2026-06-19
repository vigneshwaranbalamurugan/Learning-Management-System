using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IUserRepository : IRepository<int, Users>
    {
        Task<Users?> GetByEmailAsync(string email);
        Task<bool> IsEmailAlreadyRegisteredAsync(string email);
    }
}