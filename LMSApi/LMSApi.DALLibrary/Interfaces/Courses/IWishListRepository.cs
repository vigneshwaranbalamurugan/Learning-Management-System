using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IWishListRepository : IRepository<int, WishList>
    {
        Task RemoveAsync(int userId, int courseId);
        Task<IEnumerable<WishList>> GetByUserAsync(int userId);
        Task<bool> CheckExistsAsync(int userId, int courseId);
    }
}
