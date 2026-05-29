using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Utils;
using Microsoft.Extensions.Configuration;

namespace LMSApi.BALLibrary.Services.Upload
{
	public class UploadService : IUploadService
	{
		private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
		{
			".jpg",
			".jpeg",
			".png"
		};

		private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
		{
			"image/jpeg",
			"image/jpg",
			"image/png"
		};

		private readonly IConfiguration _configuration;

		public UploadService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public bool IsAllowedProfileImage(string fileName, string contentType)
		{
			var extension = Path.GetExtension(fileName);
			return AllowedExtensions.Contains(extension)
			       && (string.IsNullOrWhiteSpace(contentType) || AllowedContentTypes.Contains(contentType));
		}

		public Task<string> UploadProfileImageAsync(Stream fileStream, string fileName, string publicId)
		{
			var extension = Path.GetExtension(fileName);
			var contentType = extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
				? "image/png"
				: "image/jpeg";

			if (!IsAllowedProfileImage(fileName, contentType))
			{
				throw new InvalidOperationException("Only JPG, JPEG, and PNG profile pictures are allowed.");
			}

			return CloudinaryUtils.UploadProfileImageAsync(_configuration, fileStream, fileName, publicId);
		}
	}
}
