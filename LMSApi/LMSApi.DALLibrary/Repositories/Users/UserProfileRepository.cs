using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
	public class UserProfileRepository : AbstractRepository<int, UserProfiles>, IUserProfileRepository
	{
		public UserProfileRepository(LMSDbContext context) : base(context)
		{
		}

		public async Task<UserProfiles?> GetByUserIdAsync(int userId)
		{
			return await _context.UserProfiles.FirstOrDefaultAsync(profile => profile.UserId == userId);
		}
	}
}
