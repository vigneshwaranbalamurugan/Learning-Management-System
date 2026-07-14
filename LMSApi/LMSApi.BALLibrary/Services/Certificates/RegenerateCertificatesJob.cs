using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using Microsoft.Extensions.Logging;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.BALLibrary.Services
{
    public class RegenerateCertificatesJob : IRegenerateCertificatesJob
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IUploadService _uploadService;
        private readonly ICertificateService _certificateService;
        private readonly IUserNotificationsService _userNotificationsService;
        private readonly ILogger<RegenerateCertificatesJob> _logger;

        public RegenerateCertificatesJob(
            ICertificateRepository certificateRepository,
            ICourseRepository courseRepository,
            IUserRepository userRepository,
            IUserProfileRepository userProfileRepository,
            IUploadService uploadService,
            ICertificateService certificateService,
            IUserNotificationsService userNotificationsService,
            ILogger<RegenerateCertificatesJob> logger)
        {
            _certificateRepository = certificateRepository;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _userProfileRepository = userProfileRepository;
            _uploadService = uploadService;
            _certificateService = certificateService;
            _userNotificationsService = userNotificationsService;
            _logger = logger;
        }

        public async Task ExecuteAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Regeneration job aborted: User {UserId} not found.", userId);
                    return;
                }

                var learnerName = userProfile != null ? $"{userProfile.FirstName} {userProfile.LastName}".Trim() : user.Email.Split('@')[0];
                var certs = await _certificateRepository.GetCertificatesByUserAsync(userId);
                var activeTemplate = await _certificateRepository.GetActiveTemplateAsync();

                if (activeTemplate == null)
                {
                    _logger.LogWarning("Regeneration job aborted: No active template found.");
                    return;
                }

                int successCount = 0;

                foreach (var cert in certs)
                {
                    try
                    {
                        var course = await _courseRepository.GetCourseWithDetailsAsync(cert.CourseId);
                        if (course == null) continue;

                        var instructor = await _userRepository.GetByIdAsync(course.InstructorId);
                        var instructorProfile = await _userProfileRepository.GetByUserIdAsync(course.InstructorId);
                        var instructorName = instructorProfile != null ? $"{instructorProfile.FirstName} {instructorProfile.LastName}".Trim() : instructor?.Email?.Split('@')[0] ?? "Instructor";

                        string issuedDate = cert.IssuedAt.ToString("MMMM dd, yyyy");

                        using var memoryStream = await _certificateService.GenerateCertificatePdfAsync(
                            activeTemplate, course.Title, learnerName, instructorName, cert.CertificateId.ToString(), issuedDate);

                        var fileName = $"cert_{cert.CertificateId}.pdf";
                        var certBlobPath = await _uploadService.UploadCertificatePdfAsync(memoryStream, fileName, cert.CertificateId.ToString());

                        cert.CertificateImageUrl = certBlobPath;   // store blob path; served via GenerateSasUrl at access time
                        cert.CertificateTemplateId = activeTemplate.Id;
                        
                        // We keep the original IssuedAt, but update the repository
                        await _certificateRepository.UpdateCertificateAsync(cert);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to regenerate certificate {CertificateId} for User {UserId}", cert.Id, userId);
                    }
                }

                if (successCount > 0)
                {
                    await _userNotificationsService.CreateAndSendNotificationAsync(
                        userId: userId,
                        title: "Certificates Regenerated",
                        message: $"Successfully updated {successCount} certificates with your new name.",
                        type: NotificationType.CertificateIssued,
                        redirectUrl: "/learner/certificates");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in RegenerateCertificatesJob for User {UserId}", userId);
            }
        }
    }
}
