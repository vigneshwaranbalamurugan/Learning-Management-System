using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IUserNotificationsRepository : IRepository<int, Notifications>
    {
        Task<IEnumerable<Notifications>> GetByUserIdAsync(int userId);
        Task<int> GetUnreadCountByUserIdAsync(int userId);
        Task MarkAllAsReadAsync(int userId);
    }
}
