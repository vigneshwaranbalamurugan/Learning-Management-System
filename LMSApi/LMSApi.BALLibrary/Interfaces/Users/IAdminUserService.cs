using LMSApi.ModelLibrary.DTOs.UserManagement;
using System.Threading.Tasks;

namespace LMSApi.BALLibrary.Interfaces.Users
{
    public interface IAdminUserService
    {
        Task<PagedUserListResponse> GetUsersPagedAsync(UserSearchQuery query);
        Task<AdminUserResponse> CreateUserAsync(CreateUserRequest request);
    }
}
