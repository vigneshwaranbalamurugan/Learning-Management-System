using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
	public interface IUserProfileRepository : IRepository<int, UserProfiles>
	{
		Task<UserProfiles?> GetByUserIdAsync(int userId);
	}
}
