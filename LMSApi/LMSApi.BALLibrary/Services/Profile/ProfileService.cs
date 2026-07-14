using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace LMSApi.BALLibrary.Services
{
	public class ProfileService : IProfileService
	{
		private const string CacheKeyPrefix = "profile:";

		private readonly IUserRepository _userRepository;
		private readonly IUserProfileRepository _userProfileRepository;
		private readonly IUploadService _uploadService;
		private readonly IMapper _mapper;
		private readonly ICacheService _cacheService;
		private readonly int _ttlMinutes;
		private readonly ILogger<ProfileService>? _logger;

		public ProfileService(
			IUserRepository userRepository, 
			IUserProfileRepository userProfileRepository, 
			IUploadService uploadService, 
			IMapper mapper,
			ICacheService cacheService,
			IConfiguration configuration,
			ILogger<ProfileService>? logger = null)
		{
			_userRepository = userRepository;
			_userProfileRepository = userProfileRepository;
			_uploadService = uploadService;
			_mapper = mapper;
			_cacheService = cacheService;
			_ttlMinutes = configuration.GetValue<int>("Cache:ProfileTtlMinutes", 15);
			_logger = logger;
		}

		public async Task<ProfileResponse> GetProfileAsync(string email)
		{
			if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be null or empty.", nameof(email));

			_logger?.LogInformation("Retrieving profile for email: {Email}", email);

			var user = await _userRepository.GetByEmailAsync(email);
			var cacheKey = $"{CacheKeyPrefix}{user.Id}";

			var response = await _cacheService.GetOrSetAsync(
				cacheKey,
				async () =>
				{
					var profile = await _userProfileRepository.GetByUserIdAsync(user.Id);
					if (profile is null)
					{
						_logger?.LogInformation("No profile found for user ID: {UserId}. Creating default profile.", user.Id);
						profile = CreateDefaultProfile(user.Id);
						await _userProfileRepository.AddAsync(profile);
					}
					var r = _mapper.Map<ProfileResponse>(profile);
					r.Email = user.Email;
					r.Role = user.Role?.RoleName;
					return r;
				},
				TimeSpan.FromMinutes(_ttlMinutes));

			// Resolve SAS URL outside the cache factory so the cached value stays as a blob path
			// (SAS URLs expire; cache TTL < SAS expiry so this is safe).
			if (!string.IsNullOrWhiteSpace(response.ProfilePictureUrl)
			    && !response.ProfilePictureUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
			{
				response.ProfilePictureUrl = _uploadService.GeneratePublicSasUrl(response.ProfilePictureUrl);
			}

			return response;
		}

		public async Task<ProfileResponse> UpdateProfileAsync(string email, ProfileUpdateRequest request)
		{
			if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be null or empty.", nameof(email));
			if (request == null) throw new ArgumentNullException(nameof(request));
			if (string.IsNullOrWhiteSpace(request.FirstName)) throw new ArgumentException("First name cannot be null or empty.", nameof(request.FirstName));

			_logger?.LogInformation("Updating profile for email: {Email}", email);

			var user = await _userRepository.GetByEmailAsync(email);
			var profile = await _userProfileRepository.GetByUserIdAsync(user.Id);

			if (profile is null)
			{
				_logger?.LogInformation("No profile found for user ID: {UserId} during update. Creating default profile.", user.Id);
				profile = CreateDefaultProfile(user.Id);
				await _userProfileRepository.AddAsync(profile);
			}

			var oldFirstName = profile.FirstName;
			var oldLastName = profile.LastName;

			profile.FirstName = request.FirstName.Trim();
			profile.LastName = string.IsNullOrWhiteSpace(request.LastName) ? profile.LastName : request.LastName.Trim();
			
			if (oldFirstName != profile.FirstName || oldLastName != profile.LastName)
			{
				profile.NameLastChangedAt = DateTime.UtcNow;
			}

			profile.Bio = string.IsNullOrWhiteSpace(request.Bio) ? profile.Bio : request.Bio.Trim();
			profile.Location = string.IsNullOrWhiteSpace(request.Location) ? profile.Location : request.Location.Trim();
			if (request.DateOfBirth != default)
			{
				profile.DateOfBirth = request.DateOfBirth;
			}

			await _userProfileRepository.UpdateAsync(profile);
			_logger?.LogInformation("Profile updated successfully for user ID: {UserId}", user.Id);
			
			await _cacheService.InvalidateAsync($"{CacheKeyPrefix}{user.Id}");
			
			var response = _mapper.Map<ProfileResponse>(profile);
			response.Email = user.Email;
			response.Role = user.Role?.RoleName;
			return response;
		}

		public async Task<ProfileResponse> UpdateProfileImageAsync(string email, Stream fileStream, string fileName, string contentType)
		{
			if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be null or empty.", nameof(email));
			if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
			if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

			_logger?.LogInformation("Updating profile image for email: {Email}, File: {FileName}", email, fileName);

			var user = await _userRepository.GetByEmailAsync(email);

			if (!_uploadService.IsAllowedProfileImage(fileName, contentType))
			{
				_logger?.LogWarning("Failed to update profile image: disallowed file extension or type for user: {Email}", email);
				throw new InvalidOperationException("Only JPG, JPEG, and PNG profile pictures are allowed.");
			}

			var profile = await _userProfileRepository.GetByUserIdAsync(user.Id);
			if (profile is null)
			{
				_logger?.LogInformation("No profile found for user ID: {UserId} during image update. Creating default profile.", user.Id);
				profile = CreateDefaultProfile(user.Id);
				await _userProfileRepository.AddAsync(profile);
			}

			var publicId = $"profiles/{user.Id}/profile-picture";
			// Upload returns a blob path; store the path in the DB.
			var blobPath = await _uploadService.UploadProfileImageAsync(fileStream, fileName, publicId);
			profile.ProfilePictureUrl = blobPath;
			await _userProfileRepository.UpdateAsync(profile);

			_logger?.LogInformation("Profile image updated successfully for user ID: {UserId}. BlobPath: {BlobPath}", user.Id, blobPath);
			
			await _cacheService.InvalidateAsync($"{CacheKeyPrefix}{user.Id}");

			var response = _mapper.Map<ProfileResponse>(profile);
			// Serve the caller a ready-to-use SAS URL.
			response.ProfilePictureUrl = _uploadService.GeneratePublicSasUrl(blobPath);
			response.Email = user.Email;
			response.Role = user.Role?.RoleName;
			return response;
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
