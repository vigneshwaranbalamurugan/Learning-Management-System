using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Utils;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using Microsoft.Extensions.Logging;

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

		// ─── Assignment Attachments ───────────────────────────────────────────────
		private static readonly HashSet<string> AllowedAssignmentExtensions = new(StringComparer.OrdinalIgnoreCase)
		{
			".pdf", ".doc", ".docx", ".zip", ".jpg", ".jpeg", ".png", ".txt"
		};

		private readonly IConfiguration _configuration;
		private readonly ILogger<UploadService>? _logger;

		public UploadService(IConfiguration configuration, ILogger<UploadService>? logger = null)
		{
			_configuration = configuration;
			_logger = logger;
		}

				// ─── Profile image ────────────────────────────────────────────────────────
		public bool IsAllowedProfileImage(string fileName, string contentType)
		{
			if (string.IsNullOrWhiteSpace(fileName)) return false;
			var extension = Path.GetExtension(fileName);
			_logger?.LogInformation("Checking profile image: {FileName}, extension: {Extension}, content type: {ContentType}", fileName, extension, contentType);
			return AllowedImageExtensions.Contains(extension)
			       && (string.IsNullOrWhiteSpace(contentType)
			           || AllowedImageContentTypes.Contains(contentType)
			           || contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase));
		}

		public async Task<string> UploadProfileImageAsync(Stream fileStream, string fileName, string publicId)
		{
			if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
			if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
			if (string.IsNullOrWhiteSpace(publicId)) throw new ArgumentException("Public ID cannot be null or empty.", nameof(publicId));

			_logger?.LogInformation("Uploading profile image for Public ID: {PublicId}, File Name: {FileName}", publicId, fileName);

			var extension = Path.GetExtension(fileName);
			var contentType = extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
				? "image/png"
				: "image/jpeg";

			if (!IsAllowedProfileImage(fileName, contentType))
			{
				_logger?.LogWarning("Profile image upload rejected: Disallowed file type or extension for file {FileName}", fileName);
				throw new InvalidOperationException("Only JPG, JPEG, and PNG profile pictures are allowed.");
			}

			// Convert the image to WebP format
			using var image = await SixLabors.ImageSharp.Image.LoadAsync(fileStream);
			var outputStream = new MemoryStream();
			await image.SaveAsWebpAsync(outputStream);
			outputStream.Position = 0;

			var webpFileName = Path.ChangeExtension(fileName, ".webp");

			var url = await CloudinaryUtils.UploadProfileImageAsync(_configuration, outputStream, webpFileName, publicId);
			_logger?.LogInformation("Profile image uploaded successfully to Cloudinary. URL: {Url}", url);
			return url;
		}

		// ─── Course thumbnail ─────────────────────────────────────────────────────
		public bool IsAllowedCourseThumbnail(string fileName, string contentType)
		{
			if (string.IsNullOrWhiteSpace(fileName)) return false;
			var extension = Path.GetExtension(fileName);
			return AllowedImageExtensions.Contains(extension)
			       && (string.IsNullOrWhiteSpace(contentType)
			           || AllowedImageContentTypes.Contains(contentType)
			           || contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase));
		}

		public Task<string> UploadCourseThumbnailAsync(Stream fileStream, string fileName, string publicId)
		{
			if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
			if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
			if (string.IsNullOrWhiteSpace(publicId)) throw new ArgumentException("Public ID cannot be null or empty.", nameof(publicId));

			if (!IsAllowedCourseThumbnail(fileName, string.Empty))
				throw new InvalidOperationException("Only JPG, JPEG, and PNG images are allowed as course thumbnails.");

			return CloudinaryUtils.UploadCourseThumbnailAsync(_configuration, fileStream, fileName, publicId);
		}

		// ─── Course intro video ───────────────────────────────────────────────────
		public bool IsAllowedCourseVideo(string fileName, string contentType)
		{
			if (string.IsNullOrWhiteSpace(fileName)) return false;
			var extension = Path.GetExtension(fileName);
			return AllowedVideoExtensions.Contains(extension)
			       && (string.IsNullOrWhiteSpace(contentType) || AllowedVideoContentTypes.Contains(contentType));
		}

		public Task<string> UploadCourseIntroVideoAsync(Stream fileStream, string fileName, string publicId)
		{
			if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
			if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
			if (string.IsNullOrWhiteSpace(publicId)) throw new ArgumentException("Public ID cannot be null or empty.", nameof(publicId));

			if (!IsAllowedCourseVideo(fileName, string.Empty))
				throw new InvalidOperationException("Only MP4, MOV, AVI, and WEBM videos are allowed as course intro videos.");

			return CloudinaryUtils.UploadCourseIntroVideoAsync(_configuration, fileStream, fileName, publicId);
		}

		// ─── Lesson uploads ───────────────────────────────────────────────────────
		public bool IsAllowedLessonPdf(string fileName, string contentType)
		{
			if (string.IsNullOrWhiteSpace(fileName)) return false;
			var extension = Path.GetExtension(fileName);
			return AllowedPdfExtensions.Contains(extension)
			       && (string.IsNullOrWhiteSpace(contentType) || AllowedPdfContentTypes.Contains(contentType));
		}

		public Task<string> UploadLessonVideoAsync(Stream fileStream, string fileName, string publicId)
		{
			if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
			if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
			if (string.IsNullOrWhiteSpace(publicId)) throw new ArgumentException("Public ID cannot be null or empty.", nameof(publicId));

			if (!IsAllowedCourseVideo(fileName, string.Empty))
				throw new InvalidOperationException("Only MP4, MOV, AVI, and WEBM videos are allowed as lesson videos.");

			return CloudinaryUtils.UploadLessonVideoAsync(_configuration, fileStream, fileName, publicId);
		}

		public Task<string> UploadLessonPdfAsync(Stream fileStream, string fileName, string publicId)
		{
			if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
			if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
			if (string.IsNullOrWhiteSpace(publicId)) throw new ArgumentException("Public ID cannot be null or empty.", nameof(publicId));

			if (!IsAllowedLessonPdf(fileName, string.Empty))
				throw new InvalidOperationException("Only PDF files are allowed as lesson documents.");

			return CloudinaryUtils.UploadLessonPdfAsync(_configuration, fileStream, fileName, publicId);
		}

		// ─── Assignment Attachments ───────────────────────────────────────────────
		public bool IsAllowedAssignmentAttachment(string fileName, string contentType)
		{
			if (string.IsNullOrWhiteSpace(fileName)) return false;
			var extension = Path.GetExtension(fileName);
			return AllowedAssignmentExtensions.Contains(extension);
		}

		public Task<string> UploadAssignmentAttachmentAsync(Stream fileStream, string fileName, string publicId)
		{
			if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
			if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
			if (string.IsNullOrWhiteSpace(publicId)) throw new ArgumentException("Public ID cannot be null or empty.", nameof(publicId));

			if (!IsAllowedAssignmentAttachment(fileName, string.Empty))
				throw new InvalidOperationException("Invalid file type for assignment attachment.");

			return CloudinaryUtils.UploadAssignmentAttachmentAsync(_configuration, fileStream, fileName, publicId);
		}

		// ─── Certificates ─────────────────────────────────────────────────────────
		public bool IsAllowedCertificateTemplateBackground(string fileName, string contentType)
		{
			if (string.IsNullOrWhiteSpace(fileName)) return false;
			var extension = Path.GetExtension(fileName);
			return AllowedImageExtensions.Contains(extension)
			       && (string.IsNullOrWhiteSpace(contentType)
			           || AllowedImageContentTypes.Contains(contentType)
			           || contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase));
		}

		public Task<string> UploadCertificateTemplateBackgroundAsync(Stream fileStream, string fileName, string publicId)
		{
			if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
			if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
			if (string.IsNullOrWhiteSpace(publicId)) throw new ArgumentException("Public ID cannot be null or empty.", nameof(publicId));

			if (!IsAllowedCertificateTemplateBackground(fileName, string.Empty))
				throw new InvalidOperationException("Only JPG, JPEG, and PNG images are allowed as certificate template backgrounds.");

			return CloudinaryUtils.UploadCertificateTemplateBackgroundAsync(_configuration, fileStream, fileName, publicId);
		}

		public Task<string> UploadCertificatePdfAsync(Stream fileStream, string fileName, string publicId)
		{
			// Cloudinary needs to know the type for proper transformation, usually auto works, but for specific folder/naming we use utils
			return CloudinaryUtils.UploadCertificatePdfAsync(_configuration, fileStream, fileName, publicId);
		}
	}
}
