using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Repositories;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services.Notification
{
    public interface IDeadlineNotificationJob
    {
        Task ExecuteAsync();
    }

    public class DeadlineNotificationJob : IDeadlineNotificationJob
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<DeadlineNotificationJob> _logger;

        public DeadlineNotificationJob(
            INotificationRepository notificationRepository,
            INotificationService notificationService,
            ILogger<DeadlineNotificationJob> logger)
        {
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("DeadlineNotificationJob starting...");
            var tomorrow = DateTime.UtcNow.AddDays(1).Date;

            var upcomingDeadlines = await _notificationRepository.GetUpcomingDeadlinesAsync(tomorrow);
            
            _logger.LogInformation($"Found {upcomingDeadlines.Count} upcoming deadlines for {tomorrow:yyyy-MM-dd}");

            foreach (var deadline in upcomingDeadlines)
            {
                var subject = $"Upcoming Deadline for {deadline.CourseName}";
                var body = $"Hi {deadline.UserName},\n\nThis is a gentle reminder that your {deadline.ItemType} '{deadline.ItemTitle}' in the course '{deadline.CourseName}' is due tomorrow ({deadline.DeadlineDate:yyyy-MM-dd}).\n\nPlease make sure to complete it on time.\n\nBest,\nLMS Team";
                
                var emailMessage = new EmailMessage(deadline.UserEmail, subject, body);
                
                try
                {
                    await _notificationService.Send(emailMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send deadline notification email to {deadline.UserEmail}");
                }
            }

            _logger.LogInformation("DeadlineNotificationJob completed.");
        }
    }
}
