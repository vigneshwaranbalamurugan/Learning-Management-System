using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;
using Microsoft.EntityFrameworkCore;
using System;

namespace LMSApi.DALLibrary.Repositories
{
    public class ActivityLogsRepository : AbstractRepository<int, ActivityLogs>, IActivityLogsRepository
    {
        public ActivityLogsRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ActivityLogs>> GetFilteredLogsAsync(string? userQuery, string? activityType, int page, int pageSize)
        {
            var query = _context.ActivityLogs.Include(l => l.User).AsQueryable();

            if (!string.IsNullOrEmpty(userQuery))
            {
                query = query.Where(l => 
                    l.User != null && (
                        l.User.Email.Contains(userQuery) || 
                        l.UserId.ToString() == userQuery
                    )
                );
            }

            if (!string.IsNullOrEmpty(activityType) && Enum.TryParse<ActivityType>(activityType, out var typeEnum))
            {
                query = query.Where(l => l.ActivityType == typeEnum);
            }

            return await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetFilteredLogsCountAsync(string? userQuery, string? activityType)
        {
            var query = _context.ActivityLogs.Include(l => l.User).AsQueryable();

            if (!string.IsNullOrEmpty(userQuery))
            {
                query = query.Where(l => 
                    l.User != null && (
                        l.User.Email.Contains(userQuery) || 
                        l.UserId.ToString() == userQuery
                    )
                );
            }

            if (!string.IsNullOrEmpty(activityType) && Enum.TryParse<ActivityType>(activityType, out var typeEnum))
            {
                query = query.Where(l => l.ActivityType == typeEnum);
            }

            return await query.CountAsync();
        }
    }
}
