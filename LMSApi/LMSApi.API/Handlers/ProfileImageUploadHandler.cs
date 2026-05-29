using LMSApi.BALLibrary.Interfaces;

namespace LMSApi.API.Handlers
{
    public class ProfileImageUploadHandler
    {
        private readonly IUploadService _uploadService;

        public ProfileImageUploadHandler(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        public void Validate(IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                throw new InvalidOperationException("Profile picture is required.");
            }

            if (!_uploadService.IsAllowedProfileImage(file.FileName, file.ContentType ?? string.Empty))
            {
                throw new InvalidOperationException("Only JPG, JPEG, and PNG profile pictures are allowed.");
            }
        }
    }
}
