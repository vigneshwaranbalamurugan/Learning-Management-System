using LMSApi.ModelLibrary.DTOs.Logs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IAdminLogService
    {
        Task<IEnumerable<ActivityLogResponse>> GetActivityLogsAsync(int? userId, string? activityType, int page, int pageSize);
        Task<IEnumerable<AuditLogResponse>> GetAuditLogsAsync(int? userId, string? tableName, string? action, int page, int pageSize);
    }
}
