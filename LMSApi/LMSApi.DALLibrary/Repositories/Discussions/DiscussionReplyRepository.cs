using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class DiscussionReplyRepository : AbstractRepository<int, DiscussionReplies>, IDiscussionReplyRepository
    {
        public DiscussionReplyRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DiscussionReplies>> GetByDiscussionAsync(int discussionId)
        {
            return await _context.DiscussionReplies
                .IgnoreQueryFilters()
                .Include(r => r.User)
                .Where(r => r.DiscussionId == discussionId)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
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
