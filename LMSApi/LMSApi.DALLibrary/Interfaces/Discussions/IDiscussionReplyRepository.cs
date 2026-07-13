using LMSApi.ModelLibrary.Models;
namespace LMSApi.DALLibrary.Interfaces
{
    public interface IDiscussionReplyRepository : IRepository<int, DiscussionReplies>
    {
        Task<IEnumerable<DiscussionReplies>> GetByDiscussionAsync(int discussionId);
    }
}
