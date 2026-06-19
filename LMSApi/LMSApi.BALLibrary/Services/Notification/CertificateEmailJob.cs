using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Utils;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services.Notification
{
    public class CertificateEmailJob : ICertificateEmailJob
    {
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly ILogger<CertificateEmailJob> _logger;

        public CertificateEmailJob(
            INotificationService notificationService,
            IUserRepository userRepository,
            IUserProfileRepository userProfileRepository,
            ILogger<CertificateEmailJob> logger)
        {
            _notificationService = notificationService;
            _userRepository = userRepository;
            _userProfileRepository = userProfileRepository;
            _logger = logger;
        }

        public async Task ExecuteAsync(int userId, string courseName, string certificateImageUrl, Guid certificateId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found. Cannot send certificate email.", userId);
                    return;
                }

                var profile = await _userProfileRepository.GetByUserIdAsync(userId);
                var learnerName = profile != null ? $"{profile.FirstName} {profile.LastName}".Trim() : user.Email;

                var body = EmailTemplate.GetCertificateIssuedTemplate(learnerName, courseName, certificateImageUrl, certificateId);
                var subject = $"Your Certificate for {courseName} is Ready!";

                var msg = new EmailMessage(user.Email, subject, body) { IsHtml = true };
                await _notificationService.Send(msg);

                _logger.LogInformation("Successfully sent certificate email to {Email} for course {CourseName}", user.Email, courseName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send certificate email for user {UserId} and course {CourseName}", userId, courseName);
                throw;
            }
        }
    }
}
