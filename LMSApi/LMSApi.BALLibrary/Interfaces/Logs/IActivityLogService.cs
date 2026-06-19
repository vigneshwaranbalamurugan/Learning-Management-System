using LMSApi.ModelLibrary.Enums;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IActivityLogService
    {
        Task LogActivityAsync(int userId, ActivityType activityType, string description);
    }
}
