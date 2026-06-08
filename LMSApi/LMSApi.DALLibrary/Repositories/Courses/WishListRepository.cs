using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class WishListRepository : AbstractRepository<int, WishList>, IWishListRepository
    {
        public WishListRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task RemoveAsync(int userId, int courseId)
        {
            var item = await _context.WishLists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.CourseId == courseId);
            if (item != null)
            {
                _context.WishLists.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<WishList>> GetByUserAsync(int userId)
        {
            return await _context.WishLists
                .Include(w => w.Course)
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> CheckExistsAsync(int userId, int courseId)
        {
            return await _context.WishLists
                .AnyAsync(w => w.UserId == userId && w.CourseId == courseId);
        }
    }
}
