using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories.CourseModule
{
    public class EnrollmentRepository : AbstractRepository<int, Enrollments>, IEnrollmentRepository
    {
        public EnrollmentRepository(LMSDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<Enrollments?> GetByUserAndCourseAsync(int userId, int courseId)
        {
            return await _context.Enrollments
                .Include(e => e.Batch)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Enrollments>> GetEnrollmentsByBatchAsync(int batchId)
        {
            return await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .Where(e => e.BatchId == batchId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Enrollments>> GetEnrollmentsByUserAsync(int userId)
        {
            return await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .Include(e => e.Batch)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> HasEnrollmentsByCourseAsync(int courseId)
        {
            return await _context.Enrollments.AnyAsync(e => e.CourseId == courseId);
        }
    }
}
