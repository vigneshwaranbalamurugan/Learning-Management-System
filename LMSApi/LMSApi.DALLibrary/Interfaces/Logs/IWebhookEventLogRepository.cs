using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    /// <summary>
    /// Repository for persisting and querying raw Razorpay webhook event audit records.
    /// </summary>
    public interface IWebhookEventLogRepository
    {
        /// <summary>Inserts a new webhook event log record.</summary>
        Task AddAsync(WebhookEventLog log);

        /// <summary>
        /// Returns true if an event with the same EventType and EntityId has already been
        /// successfully processed. Used for idempotency checks.
        /// </summary>
        Task<bool> ExistsProcessedAsync(string eventType, string entityId);

        /// <summary>Returns recent webhook events for debugging (newest first).</summary>
        Task<IEnumerable<WebhookEventLog>> GetRecentAsync(int take = 50);
    }
}
