using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories.CourseModule
{
    public class CourseCategoryRepository : AbstractRepository<int, CourseCategories>, ICourseCategoryRepository
    {
        public CourseCategoryRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<CourseCategories?> GetByNameAsync(string name)
        {
            return await _context.CourseCategories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            return !await _context.CourseCategories
                .AnyAsync(c => c.Name.ToLower() == name.ToLower() && (excludeId == null || c.Id != excludeId));
        }
    }
}
