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

        public async Task<IEnumerable<AuditLogs>> GetFilteredLogsAsync(string? userQuery, string? tableName, string? action, int page, int pageSize)
        {
            var query = _context.AuditLogs.Include(l => l.User).AsQueryable();

            if (!string.IsNullOrEmpty(userQuery))
            {
                query = query.Where(l => 
                    l.User != null && (
                        l.User.Email.Contains(userQuery) || 
                        l.UserId.ToString() == userQuery
                    )
                );
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

        public async Task<int> GetFilteredLogsCountAsync(string? userQuery, string? tableName, string? action)
        {
            var query = _context.AuditLogs.Include(l => l.User).AsQueryable();

            if (!string.IsNullOrEmpty(userQuery))
            {
                query = query.Where(l => 
                    l.User != null && (
                        l.User.Email.Contains(userQuery) || 
                        l.UserId.ToString() == userQuery
                    )
                );
            }

            if (!string.IsNullOrEmpty(tableName))
            {
                query = query.Where(l => l.TableName == tableName);
            }

            if (!string.IsNullOrEmpty(action) && Enum.TryParse<ActionType>(action, out var actionEnum))
            {
                query = query.Where(l => l.Action == actionEnum);
            }

            return await query.CountAsync();
        }
    }
}
