namespace LMSApi.BALLibrary.Interfaces
{
	public interface IUploadService
	{
		bool IsAllowedProfileImage(string fileName, string contentType);
		Task<string> UploadProfileImageAsync(Stream fileStream, string fileName, string publicId);

		bool IsAllowedCourseThumbnail(string fileName, string contentType);
		bool IsAllowedCourseVideo(string fileName, string contentType);
		Task<string> UploadCourseThumbnailAsync(Stream fileStream, string fileName, string publicId);
		Task<(string Url, double DurationSeconds)> UploadCourseIntroVideoAsync(Stream fileStream, string fileName, string publicId);

		bool IsAllowedLessonPdf(string fileName, string contentType);
		Task<(string Url, double DurationSeconds)> UploadLessonVideoAsync(Stream fileStream, string fileName, string publicId);
		Task<string> UploadLessonPdfAsync(Stream fileStream, string fileName, string publicId);
		bool IsAllowedAssignmentAttachment(string fileName, string contentType);
		Task<string> UploadAssignmentAttachmentAsync(Stream fileStream, string fileName, string publicId);

		bool IsAllowedCertificateTemplateBackground(string fileName, string contentType);
		Task<string> UploadCertificateTemplateBackgroundAsync(Stream fileStream, string fileName, string publicId);
		Task<string> UploadCertificatePdfAsync(Stream fileStream, string fileName, string publicId);

		Task DeleteBlobAsync(string blobPath);
		/// <summary>
		/// Deletes a blob from the public container. Pass the blob path (not a full URL).
		/// </summary>
		Task DeletePublicBlobAsync(string blobPath);

		/// <summary>
		/// Generates a time-limited SAS URL for blobs in the <b>public</b> container
		/// (profile images, course thumbnails, certificate template backgrounds).
		/// Backwards-compatible: returns the input unchanged when it already starts with "http".
		/// </summary>
		string GeneratePublicSasUrl(string blobPath, int expiryMinutes = 60);

		/// <summary>
		/// Generates a time-limited SAS URL for blobs in the <b>private</b> container
		/// (lesson videos, lesson PDFs, assignment attachments, certificate PDFs).
		/// Backwards-compatible: returns the input unchanged when it already starts with "http".
		/// </summary>
		string GenerateSasUrl(string blobPath, int expiryMinutes = 60);
	}
}
