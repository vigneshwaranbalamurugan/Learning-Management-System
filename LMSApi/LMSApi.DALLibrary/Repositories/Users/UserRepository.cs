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

        public async Task<Users?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken) ?? null;
        }

        public async Task<IEnumerable<Users>> GetAdminUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserProfile)
                .Where(u => u.Role.RoleName.ToLower() == "admin" && u.IsActive)
                .ToListAsync();
        }
    }
}