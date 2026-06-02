using LMSApi.BALLibrary.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace LMSApi.API.Handlers
{
    /// <summary>
    /// Validates lesson video and PDF files before upload.
    /// </summary>
    public class LessonUploadHandler
    {
        private readonly IUploadService _uploadService;
        private readonly IConfiguration _configuration;

        public LessonUploadHandler(IUploadService uploadService, IConfiguration configuration)
        {
            _uploadService = uploadService;
            _configuration = configuration;
        }

        /// <summary>Validates that the video file is non-empty within size and extension limits.</summary>
        public void ValidateLessonVideo(IFormFile file)
        {
            if (file is null || file.Length == 0)
                throw new InvalidOperationException("Lesson video file is empty or missing.");

            int allowedSizeMB = _configuration["FileSizeLimits:LessonVideoInMB"] is string s ? int.Parse(s) : 500;
            if (file.Length > allowedSizeMB * 1024 * 1024)
                throw new InvalidOperationException($"Lesson video size exceeds the allowed limit. Only files up to {allowedSizeMB} MB are allowed.");

            if (!_uploadService.IsAllowedCourseVideo(file.FileName, file.ContentType ?? string.Empty))
                throw new InvalidOperationException("Only MP4, MOV, AVI, and WEBM videos are allowed as lesson videos.");
        }

        /// <summary>Validates that the PDF file is non-empty within size and extension limits.</summary>
        public void ValidateLessonPdf(IFormFile file)
        {
            if (file is null || file.Length == 0)
                throw new InvalidOperationException("Lesson PDF file is empty or missing.");

            int allowedSizeMB = _configuration["FileSizeLimits:LessonPdfInMB"] is string s ? int.Parse(s) : 50;
            if (file.Length > allowedSizeMB * 1024 * 1024)
                throw new InvalidOperationException($"Lesson PDF size exceeds the allowed limit. Only files up to {allowedSizeMB} MB are allowed.");

            if (!_uploadService.IsAllowedLessonPdf(file.FileName, file.ContentType ?? string.Empty))
                throw new InvalidOperationException("Only PDF files are allowed as lesson documents.");
        }
    }
}
