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

        public async Task<(IEnumerable<Users> Users, int TotalCount)> GetAllUsersPagedAsync(LMSApi.ModelLibrary.DTOs.UserManagement.UserSearchQuery query)
        {
            var queryable = _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserProfile)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLower();
                queryable = queryable.Where(u => 
                    u.Email.ToLower().Contains(s) || 
                    (u.UserProfile != null && u.UserProfile.FirstName.ToLower().Contains(s)) ||
                    (u.UserProfile != null && u.UserProfile.LastName.ToLower().Contains(s))
                );
            }

            if (query.RoleId.HasValue)
            {
                queryable = queryable.Where(u => u.RoleId == query.RoleId.Value);
            }

            if (query.IsActive.HasValue)
            {
                queryable = queryable.Where(u => u.IsActive == query.IsActive.Value);
            }

            var totalCount = await queryable.CountAsync();
            
            var users = await queryable
                .OrderByDescending(u => u.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (users, totalCount);
        }
    }
}