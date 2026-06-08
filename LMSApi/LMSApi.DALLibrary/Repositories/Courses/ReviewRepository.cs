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
                .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == courseId);
        }

        public async Task<IEnumerable<Reviews>> GetByCourseAsync(int courseId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.CourseId == courseId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}
