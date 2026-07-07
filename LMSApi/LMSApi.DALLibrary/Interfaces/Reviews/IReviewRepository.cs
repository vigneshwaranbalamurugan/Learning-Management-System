using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IReviewRepository : IRepository<int, Reviews>
    {
        Task<Reviews> GetByUserAndCourseAsync(int userId, int courseId);
        Task<IEnumerable<Reviews>> GetByCourseAsync(int courseId);
        Task<(IEnumerable<Reviews> Reviews, int TotalCount)> GetAllReviewsPagedAsync(int pageNumber, int pageSize, string? search, int? rating, bool? isDeleted);
        Task<(IEnumerable<Reviews> Reviews, int TotalCount, double AverageRating, Dictionary<int, int> RatingDistribution)> GetInstructorReviewsPagedAsync(int instructorId, int pageNumber, int pageSize, int? ratingFilter, int? courseId, string? search);
    }
}
