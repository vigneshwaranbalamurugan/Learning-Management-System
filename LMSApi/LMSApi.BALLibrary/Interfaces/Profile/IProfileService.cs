using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
	public interface IProfileService
	{
		Task<ProfileResponse> GetProfileAsync(string email);
		Task<ProfileResponse> UpdateProfileAsync(string email, ProfileUpdateRequest request);
		Task<ProfileResponse> UpdateProfileImageAsync(string email, Stream fileStream, string fileName, string contentType);
	}
}
