using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class CourseSectionRepository : AbstractRepository<int, CourseSection>, ICourseSectionRepository
    {
        public CourseSectionRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CourseSection>> GetSectionsByCourseAsync(int courseId)
        {
            return await _context.CourseSections
                .Where(s => s.CourseId == courseId)
                .ToListAsync();
        }
    }
}
