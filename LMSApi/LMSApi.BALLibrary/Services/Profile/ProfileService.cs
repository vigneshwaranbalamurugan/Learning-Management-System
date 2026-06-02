using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using AutoMapper;

namespace LMSApi.BALLibrary.Services.Profile
{
	public class ProfileService : IProfileService
	{
		private readonly IUserRepository _userRepository;
		private readonly IUserProfileRepository _userProfileRepository;
		private readonly IUploadService _uploadService;
        private readonly IMapper _mapper;

    public ProfileService(IUserRepository userRepository, IUserProfileRepository userProfileRepository, IUploadService uploadService, IMapper mapper)
		{
			_userRepository = userRepository;
			_userProfileRepository = userProfileRepository;
			_uploadService = uploadService;
			_mapper = mapper;
		}

		public async Task<ProfileResponse> GetProfileAsync(string email)
		{
			if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be null or empty.", nameof(email));

			var user = await _userRepository.GetByEmailAsync(email);
			var profile = await _userProfileRepository.GetByUserIdAsync(user.Id);
			if (profile is null)
			{
				profile = CreateDefaultProfile(user.Id);
				await _userProfileRepository.AddAsync(profile);
			}
			return _mapper.Map<ProfileResponse>(profile);
		}

		public async Task<ProfileResponse> UpdateProfileAsync(string email, ProfileUpdateRequest request)
		{
			if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be null or empty.", nameof(email));
			if (request == null) throw new ArgumentNullException(nameof(request));
			if (string.IsNullOrWhiteSpace(request.FirstName)) throw new ArgumentException("First name cannot be null or empty.", nameof(request.FirstName));

			var user = await _userRepository.GetByEmailAsync(email);
			var profile = await _userProfileRepository.GetByUserIdAsync(user.Id);

			if (profile is null)
			{
				profile = CreateDefaultProfile(user.Id);
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
			return _mapper.Map<ProfileResponse>(profile);
		}

		public async Task<ProfileResponse> UpdateProfileImageAsync(string email, Stream fileStream, string fileName, string contentType)
		{
			if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be null or empty.", nameof(email));
			if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
			if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

			var user = await _userRepository.GetByEmailAsync(email);

			if (!_uploadService.IsAllowedProfileImage(fileName, contentType))
			{
				throw new InvalidOperationException("Only JPG, JPEG, and PNG profile pictures are allowed.");
			}

			var profile = await _userProfileRepository.GetByUserIdAsync(user.Id);
			if (profile is null)
			{
				profile = CreateDefaultProfile(user.Id);
				await _userProfileRepository.AddAsync(profile);
			}

			var publicId = $"profiles/{user.Id}/profile-picture";
			profile.ProfilePictureUrl = await _uploadService.UploadProfileImageAsync(fileStream, fileName, publicId);
			await _userProfileRepository.UpdateAsync(profile);

			return _mapper.Map<ProfileResponse>(profile);
		}

        private static UserProfiles CreateDefaultProfile(int userId)
        {
            return new UserProfiles
            {
                UserId = userId,
                FirstName = string.Empty,
                LastName = string.Empty,
                Bio = string.Empty,
                Location = string.Empty,
                ProfilePictureUrl = string.Empty,
                DateOfBirth = default
            };
        }
	}
}
