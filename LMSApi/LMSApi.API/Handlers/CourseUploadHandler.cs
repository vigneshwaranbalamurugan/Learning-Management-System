using LMSApi.BALLibrary.Interfaces;

namespace LMSApi.API.Handlers
{
    /// <summary>
    /// Validates course thumbnail (image) and intro video files before upload.
    /// Mirrors ProfileImageUploadHandler.
    /// </summary>
    public class CourseUploadHandler
    {
        private readonly IUploadService _uploadService;
        private readonly IConfiguration _configuration;

        public CourseUploadHandler(IUploadService uploadService, IConfiguration configuration)
        {
            _uploadService = uploadService;
            _configuration = configuration;
        }

        /// <summary>Validates that the thumbnail is a non-empty image within the size limit.</summary>
        public void ValidateThumbnail(IFormFile file)
        {
            if (file is null || file.Length == 0)
                throw new InvalidOperationException("Course thumbnail is required.");

            int allowedSizeMB = _configuration["FileSizeLimits:CourseThumbnailInMB"] is string s ? int.Parse(s) : 5;
            if (file.Length > allowedSizeMB * 1024 * 1024)
                throw new InvalidOperationException($"Course thumbnail size exceeds the allowed limit. Only files up to {allowedSizeMB} MB are allowed.");

            if (!_uploadService.IsAllowedCourseThumbnail(file.FileName, file.ContentType ?? string.Empty))
                throw new InvalidOperationException("Only JPG, JPEG, and PNG images are allowed as course thumbnails.");
        }

        /// <summary>Validates that the intro video is a non-empty video file within the size limit.</summary>
        public void ValidateIntroVideo(IFormFile file)
        {
            if (file is null || file.Length == 0)
                throw new InvalidOperationException("Course intro video is required.");

            int allowedSizeMB = _configuration["FileSizeLimits:CourseVideoInMB"] is string s ? int.Parse(s) : 500;
            if (file.Length > allowedSizeMB * 1024 * 1024)
                throw new InvalidOperationException($"Course intro video size exceeds the allowed limit. Only files up to {allowedSizeMB} MB are allowed.");

            if (!_uploadService.IsAllowedCourseVideo(file.FileName, file.ContentType ?? string.Empty))
                throw new InvalidOperationException("Only MP4, MOV, AVI, and WEBM videos are allowed as course intro videos.");
        }
    }
}
