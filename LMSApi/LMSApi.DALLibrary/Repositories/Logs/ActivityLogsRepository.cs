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

        public async Task<IEnumerable<ActivityLogs>> GetFilteredLogsAsync(int? userId, string? activityType, int page, int pageSize)
        {
            var query = _context.ActivityLogs.Include(l => l.User).AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(l => l.UserId == userId.Value);
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
    }
}
