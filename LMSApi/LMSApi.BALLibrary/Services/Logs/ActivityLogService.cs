using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.BALLibrary.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IActivityLogsRepository _activityLogsRepository;

        public ActivityLogService(IActivityLogsRepository activityLogsRepository)
        {
            _activityLogsRepository = activityLogsRepository;
        }

        public async Task LogActivityAsync(int userId, ActivityType activityType, string description)
        {
            var log = new ActivityLogs
            {
                UserId = userId,
                ActivityType = activityType,
                Description = description,
                Timestamp = DateTime.UtcNow
            };

            await _activityLogsRepository.AddAsync(log);
        }
    }
}
