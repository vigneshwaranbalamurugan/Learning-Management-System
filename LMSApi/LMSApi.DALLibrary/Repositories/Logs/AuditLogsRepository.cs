using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;
using Microsoft.EntityFrameworkCore;
using System;

namespace LMSApi.DALLibrary.Repositories
{
    public class AuditLogsRepository : AbstractRepository<int, AuditLogs>, IAuditLogsRepository
    {
        public AuditLogsRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AuditLogs>> GetFilteredLogsAsync(int? userId, string? tableName, string? action, int page, int pageSize)
        {
            var query = _context.AuditLogs.Include(l => l.User).AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(l => l.UserId == userId.Value);
            }

            if (!string.IsNullOrEmpty(tableName))
            {
                query = query.Where(l => l.TableName == tableName);
            }

            if (!string.IsNullOrEmpty(action) && Enum.TryParse<ActionType>(action, out var actionEnum))
            {
                query = query.Where(l => l.Action == actionEnum);
            }

            return await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
