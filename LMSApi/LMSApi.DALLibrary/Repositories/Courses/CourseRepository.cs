using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class CourseRepository : AbstractRepository<int, Courses>, ICourseRepository
    {
        public CourseRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Courses>> GetCoursesByInstructorAsync(int instructorId)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Language)
                .Where(c => c.InstructorId == instructorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Courses>> GetCoursesByCategoryAsync(int categoryId)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Language)
                .Where(c => c.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Courses>> GetPublishedCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Language)
                .Where(c => c.Status == CourseStatus.Published)
                .ToListAsync();
        }

        public async Task<IEnumerable<Courses>> GetPendingCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Language)
                .Where(c => c.Status == CourseStatus.PendingApproval)
                .ToListAsync();
        }

        public async Task<Courses?> GetCourseWithDetailsAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Language)
                .Include(c => c.Instructor)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                        .ThenInclude(l => l.Resources)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Quizzes)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Assignments)
                .Include(c => c.Batches)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<LMSApi.ModelLibrary.DTOs.CourseRatingStatsDto> GetCourseRatingStatsAsync(int courseId)
        {
            var stats = await _context.Database
                .SqlQueryRaw<LMSApi.ModelLibrary.DTOs.CourseRatingStatsDto>("SELECT * FROM get_course_rating_stats({0})", courseId)
                .FirstOrDefaultAsync();

            return stats ?? new LMSApi.ModelLibrary.DTOs.CourseRatingStatsDto { AverageRating = 0.0, TotalReviews = 0 };
        }
    }
}
