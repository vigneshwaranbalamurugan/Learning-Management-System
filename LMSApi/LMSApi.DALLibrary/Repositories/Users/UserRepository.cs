using LMSApi.DALLibrary.Interfaces;
using LMSApi.DALLibrary.Contexts;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class UserRepository : AbstractRepository<int, Users>, IUserRepository
    {
        public UserRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<Users?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email)?? null;
        }

        public async Task<bool> IsEmailAlreadyRegisteredAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}