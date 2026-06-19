using LMSApi.DALLibrary.Contexts;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public interface INotificationRepository
    {
        Task<List<UpcomingDeadlineDto>> GetUpcomingDeadlinesAsync(DateTime targetDate);
    }

    public class NotificationRepository : INotificationRepository
    {
        private readonly LMSDbContext _context;

        public NotificationRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task<List<UpcomingDeadlineDto>> GetUpcomingDeadlinesAsync(DateTime targetDate)
        {
            return await _context.Database
                .SqlQueryRaw<UpcomingDeadlineDto>("SELECT * FROM get_upcoming_deadlines(CAST({0} AS date))", targetDate)
                .ToListAsync();
        }
    }
}
