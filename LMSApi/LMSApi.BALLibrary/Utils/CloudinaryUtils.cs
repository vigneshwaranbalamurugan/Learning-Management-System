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
		public static async Task<string> UploadCourseThumbnailAsync(IConfiguration configuration, Stream fileStream, string fileName, string publicId)
		{
			var cloudinary = CreateClient(configuration);
			var folder = configuration["Cloudinary:CourseThumbnailFolder"] ?? "lms/course-thumbnails";

			using var memoryStream = new MemoryStream();
			await fileStream.CopyToAsync(memoryStream);
			memoryStream.Position = 0;

			var uploadParams = new ImageUploadParams
			{
				File = new FileDescription(fileName, memoryStream),
				Folder = folder,
				PublicId = publicId,
				Overwrite = true,
				UniqueFilename = true
			};

			var result = await cloudinary.UploadAsync(uploadParams);

			if (result.Error != null)
				throw new InvalidOperationException(result.Error.Message);

			return result.SecureUrl?.ToString() ?? result.Url?.ToString() ?? throw new InvalidOperationException("Cloudinary upload did not return a usable URL.");
		}

		public static async Task<string> UploadCourseIntroVideoAsync(IConfiguration configuration, Stream fileStream, string fileName, string publicId)
		{
			var cloudinary = CreateClient(configuration);
			var folder = configuration["Cloudinary:CourseVideoFolder"] ?? "lms/course-videos";

			using var memoryStream = new MemoryStream();
			await fileStream.CopyToAsync(memoryStream);
			memoryStream.Position = 0;

			var uploadParams = new VideoUploadParams
			{
				File = new FileDescription(fileName, memoryStream),
				Folder = folder,
				PublicId = publicId,
				Overwrite = true,
				UniqueFilename = true
			};

			var result = await cloudinary.UploadAsync(uploadParams);

			if (result.Error != null)
				throw new InvalidOperationException(result.Error.Message);

			return result.SecureUrl?.ToString() ?? result.Url?.ToString() ?? throw new InvalidOperationException("Cloudinary upload did not return a usable URL.");
		}

		public static async Task<string> UploadLessonVideoAsync(IConfiguration configuration, Stream fileStream, string fileName, string publicId)
		{
			var cloudinary = CreateClient(configuration);
			var folder = configuration["Cloudinary:LessonVideoFolder"] ?? "lms/lesson-videos";

			using var memoryStream = new MemoryStream();
			await fileStream.CopyToAsync(memoryStream);
			memoryStream.Position = 0;

			var uploadParams = new VideoUploadParams
			{
				File = new FileDescription(fileName, memoryStream),
				Folder = folder,
				PublicId = publicId,
				Overwrite = true,
				UniqueFilename = true
			};

			var result = await cloudinary.UploadAsync(uploadParams);

			if (result.Error != null)
				throw new InvalidOperationException(result.Error.Message);

			return result.SecureUrl?.ToString() ?? result.Url?.ToString() ?? throw new InvalidOperationException("Cloudinary upload did not return a usable URL.");
		}

		public static async Task<string> UploadLessonPdfAsync(IConfiguration configuration, Stream fileStream, string fileName, string publicId)
		{
			var cloudinary = CreateClient(configuration);
			var folder = configuration["Cloudinary:LessonPdfFolder"] ?? "lms/lesson-pdfs";

			using var memoryStream = new MemoryStream();
			await fileStream.CopyToAsync(memoryStream);
			memoryStream.Position = 0;

			var extension = Path.GetExtension(fileName);
			var publicIdWithExt = string.IsNullOrWhiteSpace(extension) ? publicId : $"{publicId}{extension}";

			var uploadParams = new RawUploadParams
			{
				File = new FileDescription(fileName, memoryStream),
				Folder = folder,
				PublicId = publicIdWithExt,
				Overwrite = true
			};

			var result = await cloudinary.UploadAsync(uploadParams);

			if (result.Error != null)
				throw new InvalidOperationException(result.Error.Message);

			return result.SecureUrl?.ToString() ?? result.Url?.ToString() ?? throw new InvalidOperationException("Cloudinary upload did not return a usable URL.");
		}
		public static async Task<string> UploadAssignmentAttachmentAsync(IConfiguration configuration, Stream fileStream, string fileName, string publicId)
		{
			var cloudinary = CreateClient(configuration);
			var folder = configuration["Cloudinary:AssignmentAttachmentFolder"] ?? "lms/assignment-attachments";

			using var memoryStream = new MemoryStream();
			await fileStream.CopyToAsync(memoryStream);
			memoryStream.Position = 0;

			var extension = Path.GetExtension(fileName);
			var publicIdWithExt = string.IsNullOrWhiteSpace(extension) ? publicId : $"{publicId}{extension}";

			var uploadParams = new RawUploadParams
			{
				File = new FileDescription(fileName, memoryStream),
				Folder = folder,
				PublicId = publicIdWithExt,
				Overwrite = true
			};

			var result = await cloudinary.UploadAsync(uploadParams);

			if (result.Error != null)
				throw new InvalidOperationException(result.Error.Message);

			return result.SecureUrl?.ToString() ?? result.Url?.ToString() ?? throw new InvalidOperationException("Cloudinary upload did not return a usable URL.");
		}
		public static async Task<string> UploadCertificateTemplateBackgroundAsync(IConfiguration configuration, Stream fileStream, string fileName, string publicId)
		{
			var cloudinary = CreateClient(configuration);
			var folder = configuration["Cloudinary:CertificateTemplateFolder"] ?? "lms/certificate-templates";

			using var memoryStream = new MemoryStream();
			await fileStream.CopyToAsync(memoryStream);
			memoryStream.Position = 0;

			var uploadParams = new ImageUploadParams
			{
				File = new FileDescription(fileName, memoryStream),
				Folder = folder,
				PublicId = publicId,
				Overwrite = true,
				UniqueFilename = false
			};

			var result = await cloudinary.UploadAsync(uploadParams);

			if (result.Error != null)
				throw new InvalidOperationException(result.Error.Message);

			return result.SecureUrl?.ToString() ?? result.Url?.ToString() ?? throw new InvalidOperationException("Cloudinary upload did not return a usable URL.");
		}

		public static async Task<string> UploadCertificatePdfAsync(IConfiguration configuration, Stream fileStream, string fileName, string publicId)
		{
			var cloudinary = CreateClient(configuration);
			var folder = configuration["Cloudinary:CertificateFolder"] ?? "lms/certificates";

			using var memoryStream = new MemoryStream();
			await fileStream.CopyToAsync(memoryStream);
			memoryStream.Position = 0;

			var uploadParams = new RawUploadParams
			{
				File = new FileDescription(fileName, memoryStream),
				Folder = folder,
				PublicId = publicId,
				Overwrite = true
			};

			var result = await cloudinary.UploadAsync(uploadParams);

			if (result.Error != null)
				throw new InvalidOperationException(result.Error.Message);

			return result.SecureUrl?.ToString() ?? result.Url?.ToString() ?? throw new InvalidOperationException("Cloudinary upload did not return a usable URL.");
		}
	}
}
