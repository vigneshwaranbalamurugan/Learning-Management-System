using LMSApi.ModelLibrary.DTOs;
namespace LMSApi.BALLibrary.Interfaces
{
    public interface IDiscussionService
    {
        Task<DiscussionResponse> CreateDiscussionAsync(int userId, CreateDiscussionRequest request);
        Task<DiscussionResponse> UpdateDiscussionAsync(int userId, int discussionId, UpdateDiscussionRequest request);
        Task DeleteDiscussionAsync(int userId, int discussionId);
        Task<IEnumerable<DiscussionResponse>> GetLessonDiscussionsAsync(int lessonId);
        Task<DiscussionDetailResponse> GetDiscussionDetailAsync(int discussionId);
        Task<ReplyResponse> AddReplyAsync(int userId, int discussionId, CreateReplyRequest request);
        Task<ReplyResponse> UpdateReplyAsync(int userId, int replyId, UpdateReplyRequest request);
        Task DeleteReplyAsync(int userId, int replyId);
        Task<int> ToggleLikeAsync(int userId, int discussionId);
    }
}
