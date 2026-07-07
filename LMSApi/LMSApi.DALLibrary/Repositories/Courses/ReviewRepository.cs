using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class ReviewRepository : AbstractRepository<int, Reviews>, IReviewRepository
    {
        public ReviewRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<Reviews> GetByUserAndCourseAsync(int userId, int courseId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == courseId && !r.IsDeleted);
        }

        public async Task<IEnumerable<Reviews>> GetByCourseAsync(int courseId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.CourseId == courseId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Reviews> Reviews, int TotalCount)> GetAllReviewsPagedAsync(int pageNumber, int pageSize, string? search, int? rating, bool? isDeleted)
        {
            var queryable = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Course)
                .AsQueryable();

            if (isDeleted.HasValue)
            {
                queryable = queryable.Where(r => r.IsDeleted == isDeleted.Value);
            }

            if (rating.HasValue && rating.Value > 0)
            {
                queryable = queryable.Where(r => r.Rating == rating.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                queryable = queryable.Where(r => r.Review.ToLower().Contains(s) || 
                                                (r.User != null && r.User.UserProfile != null && r.User.UserProfile.FirstName.ToLower().Contains(s)) ||
                                                (r.Course != null && r.Course.Title.ToLower().Contains(s)));
            }

            var totalCount = await queryable.CountAsync();
            
            var reviews = await queryable
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reviews, totalCount);
        }
        public async Task<(IEnumerable<Reviews> Reviews, int TotalCount, double AverageRating, Dictionary<int, int> RatingDistribution)> GetInstructorReviewsPagedAsync(int instructorId, int pageNumber, int pageSize, int? ratingFilter, int? courseId, string? search)
        {
            var queryable = _context.Reviews
                .Include(r => r.User)
                    .ThenInclude(u => u.UserProfile)
                .Include(r => r.Course)
                .Where(r => r.Course.InstructorId == instructorId && !r.IsDeleted)
                .AsQueryable();

            if (courseId.HasValue)
            {
                queryable = queryable.Where(r => r.CourseId == courseId.Value);
            }

            if (ratingFilter.HasValue && ratingFilter.Value > 0)
            {
                queryable = queryable.Where(r => r.Rating == ratingFilter.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                queryable = queryable.Where(r => r.Review.ToLower().Contains(s) || 
                                                (r.User != null && r.User.UserProfile != null && r.User.UserProfile.FirstName.ToLower().Contains(s)) ||
                                                (r.Course != null && r.Course.Title.ToLower().Contains(s)));
            }

            var allFiltered = await queryable.ToListAsync();
            var totalCount = allFiltered.Count;
            
            double avgRating = 0;
            var distribution = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 } };
            
            if (totalCount > 0)
            {
                avgRating = allFiltered.Average(r => r.Rating);
                foreach (var r in allFiltered)
                {
                    if (distribution.ContainsKey(r.Rating))
                        distribution[r.Rating]++;
                }
            }

            var reviews = allFiltered
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (reviews, totalCount, avgRating, distribution);
        }
    }
}
