using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories.CourseModule
{
    public class LessonRepository : AbstractRepository<int, Lessons>, ILessonRepository
    {
        public LessonRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Lessons>> GetLessonsBySectionAsync(int sectionId)
        {
            return await _context.Lessons
                .Where(l => l.CourseSectionId == sectionId)
                .OrderBy(l => l.SortOrder)
                .ToListAsync();
        }
    }
}
