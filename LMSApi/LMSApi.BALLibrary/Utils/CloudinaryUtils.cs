using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace LMSApi.BALLibrary.Utils
{
	public static class CloudinaryUtils
	{
		public static Cloudinary CreateClient(IConfiguration configuration)
		{
			var cloudName = configuration["Cloudinary:CloudName"];
			var apiKey = configuration["Cloudinary:ApiKey"];
			var apiSecret = configuration["Cloudinary:ApiSecret"];

			if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
			{
				throw new InvalidOperationException("Cloudinary configuration is missing. Set Cloudinary:CloudName, Cloudinary:ApiKey, and Cloudinary:ApiSecret.");
			}

			var account = new Account(cloudName, apiKey, apiSecret);
			return new Cloudinary(account)
			{
				Api = { Secure = true }
			};
		}

		public static async Task<string> UploadProfileImageAsync(IConfiguration configuration, Stream fileStream, string fileName, string publicId)
		{
			var cloudinary = CreateClient(configuration);
			var folder = configuration["Cloudinary:Folder"] ?? "lms/profile-pictures";

			var uploadParams = new ImageUploadParams
			{
				File = new FileDescription(fileName, fileStream),
				Folder = folder,
				PublicId = publicId,
				Overwrite = true,
				UniqueFilename = true
			};

			var result = await cloudinary.UploadAsync(uploadParams);

			if (result.Error != null)
			{
				throw new InvalidOperationException(result.Error.Message);
			}

			return result.SecureUrl?.ToString() ?? result.Url?.ToString() ?? throw new InvalidOperationException("Cloudinary upload did not return a usable URL.");
		}
	}
}
