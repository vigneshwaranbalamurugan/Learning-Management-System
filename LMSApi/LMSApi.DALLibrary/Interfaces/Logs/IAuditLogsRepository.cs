using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IAuditLogsRepository : IRepository<int, AuditLogs>
    {
        Task<IEnumerable<AuditLogs>> GetFilteredLogsAsync(int? userId, string? tableName, string? action, int page, int pageSize);
    }
}
