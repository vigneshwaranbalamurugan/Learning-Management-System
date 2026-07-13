using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class DiscussionLikeRepository : AbstractRepository<int, DiscussionLikes>, IDiscussionLikeRepository
    {
        public DiscussionLikeRepository(LMSDbContext context) : base(context) { }

        public async Task<DiscussionLikes> GetByDiscussionAndUserAsync(int discussionId, int userId)
        {
            return await _context.DiscussionLikes
                .FirstOrDefaultAsync(l => l.DiscussionId == discussionId && l.UserId == userId);
        }

        public async Task<int> GetLikeCountAsync(int discussionId)
        {
            return await _context.DiscussionLikes
                .CountAsync(l => l.DiscussionId == discussionId);
        }
    }
}
