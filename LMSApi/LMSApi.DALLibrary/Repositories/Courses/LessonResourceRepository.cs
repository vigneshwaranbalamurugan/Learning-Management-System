using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class LessonResourceRepository : AbstractRepository<int, LessonResources>, ILessonResourceRepository
    {
        public LessonResourceRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<LessonResources>> GetResourcesByLessonAsync(int lessonId)
        {
            return await _context.LessonResources
                .Where(r => r.LessonId == lessonId)
                .ToListAsync();
        }
    }
}
