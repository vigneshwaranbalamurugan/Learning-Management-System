using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewResponse> AddReviewAsync(int userId, CreateReviewRequest request);
        Task<ReviewResponse> UpdateReviewAsync(int userId, int reviewId, UpdateReviewRequest request);
        Task DeleteReviewAsync(int userId, int reviewId);
        Task DeleteReviewByAdminAsync(int reviewId);
        Task<IEnumerable<ReviewResponse>> GetCourseReviewsAsync(int courseId);
        Task<PagedReviewResponse> GetAllReviewsPagedAsync(int pageNumber, int pageSize, string? search);
    }
}
