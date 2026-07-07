using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IAuditLogsRepository : IRepository<int, AuditLogs>
    {
        Task<IEnumerable<AuditLogs>> GetFilteredLogsAsync(string? userQuery, string? tableName, string? action, int page, int pageSize);
        Task<int> GetFilteredLogsCountAsync(string? userQuery, string? tableName, string? action);
        Task<AuditLogs> GetAuditLogByIdAsync(int id);
    }
}
