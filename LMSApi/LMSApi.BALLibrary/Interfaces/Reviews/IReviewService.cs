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
        Task<PagedReviewResponse> GetAllReviewsPagedAsync(int pageNumber, int pageSize, string? search, int? rating, string? status);
        Task<PagedInstructorReviewResponse> GetInstructorReviewsPagedAsync(int instructorId, int pageNumber, int pageSize, int? ratingFilter, int? courseId, string? search);
        Task RestoreReviewByAdminAsync(int reviewId);
    }
}
