using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IActivityLogsRepository : IRepository<int, ActivityLogs>
    {
        Task<IEnumerable<ActivityLogs>> GetFilteredLogsAsync(int? userId, string? activityType, int page, int pageSize);
    }
}
