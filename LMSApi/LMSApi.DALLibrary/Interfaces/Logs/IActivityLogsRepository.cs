using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IActivityLogsRepository : IRepository<int, ActivityLogs>
    {
        Task<IEnumerable<ActivityLogs>> GetFilteredLogsAsync(string? userQuery, string? activityType, int page, int pageSize);
        Task<int> GetFilteredLogsCountAsync(string? userQuery, string? activityType);
    }
}
