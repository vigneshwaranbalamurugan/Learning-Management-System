using System.Security.Cryptography.X509Certificates;
using LMSApi.BALLibrary.Interfaces;

namespace LMSApi.API.Handlers
{
    public class ProfileImageUploadHandler
    {
        private readonly IUploadService _uploadService;
        private readonly IConfiguration _configuration;

        public ProfileImageUploadHandler(IUploadService uploadService,IConfiguration configuration)
        {
            _uploadService = uploadService;
            _configuration = configuration;
        }

        public void Validate(IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                throw new InvalidOperationException("Profile picture is required.");
            }
            int allowedSize = _configuration["FileSizeLimits:ProfileImageinMB"] != null ? int.Parse(_configuration["FileSizeLimits:ProfileImageinMB"]) : 5;
            if (file.Length > allowedSize * 1024 * 1024)
            {
                throw new InvalidOperationException("Profile picture size exceeds the allowed limit.Only files up to " + allowedSize + " MB are allowed.");
            }

            if (!_uploadService.IsAllowedProfileImage(file.FileName, file.ContentType ?? string.Empty))
            {
                throw new InvalidOperationException("Only JPG, JPEG, and PNG profile pictures are allowed.");
            }
        }
    }
}
