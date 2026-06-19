using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IWishListService
    {
        Task<WishListResponse> AddToWishListAsync(int userId, AddWishListRequest request);
        Task RemoveFromWishListAsync(int userId, int courseId);
        Task<IEnumerable<WishListResponse>> GetUserWishListAsync(int userId);
    }
}
