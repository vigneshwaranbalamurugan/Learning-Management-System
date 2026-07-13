using LMSApi.ModelLibrary.Models;
namespace LMSApi.DALLibrary.Interfaces
{
    public interface IDiscussionLikeRepository : IRepository<int, DiscussionLikes>
    {
        Task<DiscussionLikes> GetByDiscussionAndUserAsync(int discussionId, int userId);
        Task<int> GetLikeCountAsync(int discussionId);
    }
}
