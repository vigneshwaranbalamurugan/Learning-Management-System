namespace LMSApi.BALLibrary.Interfaces
{
	public interface IUploadService
	{
		bool IsAllowedProfileImage(string fileName, string contentType);
		Task<string> UploadProfileImageAsync(Stream fileStream, string fileName, string publicId);
	}
}
