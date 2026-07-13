using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class DiscussionRepository : AbstractRepository<int, Discussions>, IDiscussionRepository
    {
        public DiscussionRepository(LMSDbContext context) : base(context) { }

        public async Task<IEnumerable<Discussions>> GetByLessonAsync(int lessonId)
        {
            return await _context.Discussions
                .Include(d => d.User)
                .Where(d => d.LessonId == lessonId && !d.IsDeleted)
                .OrderByDescending(d => d.IsPinned)
                .ThenByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<Discussions> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Discussions
                .IgnoreQueryFilters()
                .Include(d => d.User)
                .Include(d => d.Lesson)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public override async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                await UpdateAsync(entity);
            }
        }
    }
}
