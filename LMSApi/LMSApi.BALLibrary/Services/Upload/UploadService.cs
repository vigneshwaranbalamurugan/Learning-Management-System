using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Utils;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;

namespace LMSApi.BALLibrary.Services.Upload
{
	public class UploadService : IUploadService
	{
		// ─── Profile image ───────────────────────────────────────────────────────
		private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
		{
			".jpg",
			".jpeg",
			".png"
		};

		private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
		{
			"image/jpeg",
			"image/jpg",
			"image/png"
		};

		// ─── Course video ─────────────────────────────────────────────────────────
		private static readonly HashSet<string> AllowedVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
		{
			".mp4",
			".mov",
			".avi",
			".webm"
		};

		private static readonly HashSet<string> AllowedVideoContentTypes = new(StringComparer.OrdinalIgnoreCase)
		{
			"video/mp4",
			"video/quicktime",
			"video/x-msvideo",
			"video/webm"
		};

		// ─── PDF Document ─────────────────────────────────────────────────────────
		private static readonly HashSet<string> AllowedPdfExtensions = new(StringComparer.OrdinalIgnoreCase)
		{
			".pdf"
		};

		private static readonly HashSet<string> AllowedPdfContentTypes = new(StringComparer.OrdinalIgnoreCase)
		{
			"application/pdf"
		};

		private readonly IConfiguration _configuration;

		public UploadService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		// ─── Profile image ────────────────────────────────────────────────────────
		public bool IsAllowedProfileImage(string fileName, string contentType)
		{
			var extension = Path.GetExtension(fileName);
			return AllowedImageExtensions.Contains(extension)
			       && (string.IsNullOrWhiteSpace(contentType) || AllowedImageContentTypes.Contains(contentType));
		}

		public async Task<string> UploadProfileImageAsync(Stream fileStream, string fileName, string publicId)
		{
			var extension = Path.GetExtension(fileName);
			var contentType = extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
				? "image/png"
				: "image/jpeg";

			if (!IsAllowedProfileImage(fileName, contentType))
				throw new InvalidOperationException("Only JPG, JPEG, and PNG profile pictures are allowed.");

			// Convert the image to WebP format
			using var image = await SixLabors.ImageSharp.Image.LoadAsync(fileStream);
			var outputStream = new MemoryStream();
			await image.SaveAsWebpAsync(outputStream);
			outputStream.Position = 0;

			var webpFileName = Path.ChangeExtension(fileName, ".webp");

			return await CloudinaryUtils.UploadProfileImageAsync(_configuration, outputStream, webpFileName, publicId);
		}

		// ─── Course thumbnail ─────────────────────────────────────────────────────
		public bool IsAllowedCourseThumbnail(string fileName, string contentType)
		{
			var extension = Path.GetExtension(fileName);
			return AllowedImageExtensions.Contains(extension)
			       && (string.IsNullOrWhiteSpace(contentType) || AllowedImageContentTypes.Contains(contentType));
		}

		public Task<string> UploadCourseThumbnailAsync(Stream fileStream, string fileName, string publicId)
		{
			if (!IsAllowedCourseThumbnail(fileName, string.Empty))
				throw new InvalidOperationException("Only JPG, JPEG, and PNG images are allowed as course thumbnails.");

			return CloudinaryUtils.UploadCourseThumbnailAsync(_configuration, fileStream, fileName, publicId);
		}

		// ─── Course intro video ───────────────────────────────────────────────────
		public bool IsAllowedCourseVideo(string fileName, string contentType)
		{
			var extension = Path.GetExtension(fileName);
			return AllowedVideoExtensions.Contains(extension)
			       && (string.IsNullOrWhiteSpace(contentType) || AllowedVideoContentTypes.Contains(contentType));
		}

		public Task<string> UploadCourseIntroVideoAsync(Stream fileStream, string fileName, string publicId)
		{
			if (!IsAllowedCourseVideo(fileName, string.Empty))
				throw new InvalidOperationException("Only MP4, MOV, AVI, and WEBM videos are allowed as course intro videos.");

			return CloudinaryUtils.UploadCourseIntroVideoAsync(_configuration, fileStream, fileName, publicId);
		}

		// ─── Lesson uploads ───────────────────────────────────────────────────────
		public bool IsAllowedLessonPdf(string fileName, string contentType)
		{
			var extension = Path.GetExtension(fileName);
			return AllowedPdfExtensions.Contains(extension)
			       && (string.IsNullOrWhiteSpace(contentType) || AllowedPdfContentTypes.Contains(contentType));
		}

		public Task<string> UploadLessonVideoAsync(Stream fileStream, string fileName, string publicId)
		{
			if (!IsAllowedCourseVideo(fileName, string.Empty))
				throw new InvalidOperationException("Only MP4, MOV, AVI, and WEBM videos are allowed as lesson videos.");

			return CloudinaryUtils.UploadLessonVideoAsync(_configuration, fileStream, fileName, publicId);
		}

		public Task<string> UploadLessonPdfAsync(Stream fileStream, string fileName, string publicId)
		{
			if (!IsAllowedLessonPdf(fileName, string.Empty))
				throw new InvalidOperationException("Only PDF files are allowed as lesson documents.");

			return CloudinaryUtils.UploadLessonPdfAsync(_configuration, fileStream, fileName, publicId);
		}
	}
}
