using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class StudentProgressRepository : AbstractRepository<int, StudentProgress>, IStudentProgressRepository
    {
        public StudentProgressRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<StudentProgress?> GetProgressByUserAndLessonAsync(int userId, int lessonId)
        {
            return await _context.StudentProgresses
                .FirstOrDefaultAsync(p => p.StudentId == userId && p.LessonId == lessonId);
        }

        public async Task<IEnumerable<StudentProgress>> GetProgressByUserAndCourseAsync(int userId, int courseId)
        {
            return await _context.StudentProgresses
                .Include(p => p.Lesson)
                    .ThenInclude(l => l.CourseSection)
                .Where(p => p.StudentId == userId && p.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<int> GetCompletedLessonsCountAsync(int userId, int courseId)
        {
            return await _context.StudentProgresses
                .CountAsync(p => p.StudentId == userId && p.CourseId == courseId && p.IsCompleted);
        }
    }
}
