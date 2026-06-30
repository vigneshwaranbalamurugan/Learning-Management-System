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

        public async Task<(IEnumerable<Reviews> Reviews, int TotalCount)> GetAllReviewsPagedAsync(int pageNumber, int pageSize, string? search)
        {
            var queryable = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Course)
                .Where(r => !r.IsDeleted)
                .AsQueryable();

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
    }
}
