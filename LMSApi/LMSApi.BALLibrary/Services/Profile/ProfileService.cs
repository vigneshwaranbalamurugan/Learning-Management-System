using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Services.Profile
{
	public class ProfileService : IProfileService
	{
		private readonly IUserRepository _userRepository;
		private readonly IUserProfileRepository _userProfileRepository;
		private readonly IUploadService _uploadService;

		public ProfileService(IUserRepository userRepository, IUserProfileRepository userProfileRepository, IUploadService uploadService)
		{
			_userRepository = userRepository;
			_userProfileRepository = userProfileRepository;
			_uploadService = uploadService;
		}

		public async Task<ProfileResponse> GetProfileAsync(string email)
		{
			var user = await _userRepository.GetByEmailAsync(email);
			var profile = await _userProfileRepository.GetByUserIdAsync(user.Id);

			return MapToResponse(user.Email, profile);
		}

		public async Task<ProfileResponse> UpdateProfileAsync(string email, ProfileUpdateRequest request)
		{
			var user = await _userRepository.GetByEmailAsync(email);
			var profile = await _userProfileRepository.GetByUserIdAsync(user.Id);

			if (profile is null)
			{
				profile = new UserProfiles
				{
					UserId = user.Id,
					FirstName = string.Empty,
					LastName = string.Empty,
					Bio = string.Empty,
					Location = string.Empty,
					ProfilePictureUrl = string.Empty,
					DateOfBirth = default
				};
				await _userProfileRepository.AddAsync(profile);
			}

			profile.FirstName = request.FirstName.Trim();
			profile.LastName = string.IsNullOrWhiteSpace(request.LastName) ? profile.LastName : request.LastName.Trim();
			profile.Bio = string.IsNullOrWhiteSpace(request.Bio) ? profile.Bio : request.Bio.Trim();
			profile.Location = string.IsNullOrWhiteSpace(request.Location) ? profile.Location : request.Location.Trim();
			if (request.DateOfBirth != default)
			{
				profile.DateOfBirth = request.DateOfBirth;
			}

			await _userProfileRepository.UpdateAsync(profile);
			return MapToResponse(user.Email, profile);
		}

		public async Task<ProfileResponse> UpdateProfileImageAsync(string email, Stream fileStream, string fileName, string contentType)
		{
			var user = await _userRepository.GetByEmailAsync(email);

			if (!_uploadService.IsAllowedProfileImage(fileName, contentType))
			{
				throw new InvalidOperationException("Only JPG, JPEG, and PNG profile pictures are allowed.");
			}

			var profile = await _userProfileRepository.GetByUserIdAsync(user.Id);
			if (profile is null)
			{
				profile = new UserProfiles
				{
					UserId = user.Id,
					FirstName = string.Empty,
					LastName = string.Empty,
					Bio = string.Empty,
					Location = string.Empty,
					ProfilePictureUrl = string.Empty,
					DateOfBirth = default
				};
				await _userProfileRepository.AddAsync(profile);
			}

			var publicId = $"profiles/{user.Id}/profile-picture";
			profile.ProfilePictureUrl = await _uploadService.UploadProfileImageAsync(fileStream, fileName, publicId);
			await _userProfileRepository.UpdateAsync(profile);

			return MapToResponse(user.Email, profile);
		}

		private static ProfileResponse MapToResponse(string email, UserProfiles? profile)
		{
			return new ProfileResponse
			{
				Email = email,
				FirstName = profile?.FirstName ?? string.Empty,
				LastName = profile?.LastName ?? string.Empty,
				Bio = profile?.Bio ?? string.Empty,
				DateOfBirth = profile?.DateOfBirth ?? default,
				Location = profile?.Location ?? string.Empty,
				ProfilePictureUrl = profile?.ProfilePictureUrl ?? string.Empty
			};
		}
	}
}
