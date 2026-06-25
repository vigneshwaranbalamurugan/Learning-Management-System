using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class WebhookEventLogRepository : IWebhookEventLogRepository
    {
        private readonly LMSDbContext _context;

        public WebhookEventLogRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(WebhookEventLog log)
        {
            await _context.WebhookEventLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsProcessedAsync(string eventType, string entityId)
        {
            return await _context.WebhookEventLogs
                .AnyAsync(e => e.EventType == eventType
                            && e.EntityId == entityId
                            && e.Processed);
        }

        public async Task<IEnumerable<WebhookEventLog>> GetRecentAsync(int take = 50)
        {
            return await _context.WebhookEventLogs
                .OrderByDescending(e => e.ReceivedAt)
                .Take(take)
                .ToListAsync();
        }
    }
}
