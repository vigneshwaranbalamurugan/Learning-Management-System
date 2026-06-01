namespace LMSApi.BALLibrary.Interfaces
{
	public interface IUploadService
	{
		bool IsAllowedProfileImage(string fileName, string contentType);
		Task<string> UploadProfileImageAsync(Stream fileStream, string fileName, string publicId);

		bool IsAllowedCourseThumbnail(string fileName, string contentType);
		bool IsAllowedCourseVideo(string fileName, string contentType);
		Task<string> UploadCourseThumbnailAsync(Stream fileStream, string fileName, string publicId);
		Task<string> UploadCourseIntroVideoAsync(Stream fileStream, string fileName, string publicId);
	}
}
