using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.BALLibrary.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace LMSApi.BALLibrary.Services.Upload
{
    public class SecureMediaService : ISecureMediaService
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SecureMediaService> _logger;

        public SecureMediaService(
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            IConfiguration configuration,
            ILogger<SecureMediaService> logger)
        {
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetSecureUrlAsync(string blobPath, int? userId, int courseId)
        {
            if (string.IsNullOrWhiteSpace(blobPath))
                throw new ArgumentException("Blob path is required.");

            // Backward compatibility
            if (blobPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return blobPath;

            var course = await _courseRepository.GetCourseWithDetailsAsync(courseId);
            if (course == null)
                throw new KeyNotFoundException($"Course with ID {courseId} not found.");

            // Check if this blobPath belongs to a preview lesson in this course
            bool isPreview = course.Sections?
                .SelectMany(s => s.Lessons ?? Enumerable.Empty<LMSApi.ModelLibrary.Models.Lessons>())
                .Any(l => l.IsPreview && l.ContentUrl == blobPath) ?? false;

            // Allow if it's the course intro video
            if (!string.IsNullOrEmpty(course.IntroVideoUrl) && course.IntroVideoUrl == blobPath)
            {
                isPreview = true;
            }

            if (!isPreview)
            {
                if (userId == null)
                {
                    _logger.LogWarning("Unauthorized access attempt to secure media by anonymous user. CourseId: {CourseId}, BlobPath: {BlobPath}", courseId, blobPath);
                    throw new UnauthorizedAccessException("You must be logged in and enrolled in the course to access this content.");
                }

                // Allow instructor to view their own course content without enrollment
                if (course.InstructorId != userId.Value)
                {
                    // Check if user is enrolled
                    var isEnrolled = await _enrollmentRepository.IsAlreadyEnrolledAsync(userId.Value, courseId);
                    if (!isEnrolled)
                    {
                        _logger.LogWarning("Unauthorized access attempt to secure media. UserId: {UserId}, CourseId: {CourseId}, BlobPath: {BlobPath}", userId.Value, courseId, blobPath);
                        throw new UnauthorizedAccessException("You must be enrolled in the course to access this content.");
                    }
                }
            }

            // Determine expiry based on file type (video vs pdf vs other)
            int expiryMinutes = 15; // default
            if (blobPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                expiryMinutes = _configuration.GetValue<int>("AzureBlob:PdfSasTokenExpiryMinutes", 5);
            }
            else
            {
                expiryMinutes = _configuration.GetValue<int>("AzureBlob:VideoSasTokenExpiryMinutes", 15);
            }

            try
            {
                var sasUrl = AzureBlobUtils.GenerateSasUrl(_configuration, blobPath, expiryMinutes);
                return sasUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SAS URL for blob path: {BlobPath}", blobPath);
                throw new InvalidOperationException("Could not generate secure access URL for the requested media.");
            }
        }
    }
}
