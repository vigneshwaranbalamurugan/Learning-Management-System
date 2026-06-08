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
                .Where(c => c.InstructorId == instructorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Courses>> GetCoursesByCategoryAsync(int categoryId)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Where(c => c.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Courses>> GetPublishedCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Where(c => c.Status == CourseStatus.Published)
                .ToListAsync();
        }

        public async Task<Courses?> GetCourseWithDetailsAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Quizzes)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Assignments)
                .Include(c => c.Batches)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

    }
}
