using LMSApi.ModelLibrary.DTOs.Logs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IAdminLogService
    {
        Task<IEnumerable<ActivityLogResponse>> GetActivityLogsAsync(string? userQuery, string? activityType, int page, int pageSize);
        Task<int> GetActivityLogsCountAsync(string? userQuery, string? activityType);
        Task<IEnumerable<AuditLogResponse>> GetAuditLogsAsync(string? userQuery, string? tableName, string? action, int page, int pageSize);
        Task<int> GetAuditLogsCountAsync(string? userQuery, string? tableName, string? action);
    }
}
