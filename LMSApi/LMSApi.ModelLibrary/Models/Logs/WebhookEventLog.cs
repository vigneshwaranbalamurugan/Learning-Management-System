namespace LMSApi.ModelLibrary.Models
{
    /// <summary>
    /// Audit table that stores every raw Razorpay webhook event received.
    /// Used for idempotency checks, debugging, and compliance.
    /// </summary>
    public class WebhookEventLog
    {
        public int Id { get; set; }

        /// <summary>Razorpay event type string, e.g. "payment.dispute.created".</summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>Primary Razorpay entity ID extracted from the payload (order, payment, account, dispute ID, etc.).</summary>
        public string? EntityId { get; set; }

        /// <summary>Full raw JSON payload received from Razorpay.</summary>
        public string RawPayload { get; set; } = string.Empty;

        /// <summary>UTC timestamp when the event was received by this server.</summary>
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Whether the event was successfully processed without throwing an unhandled exception.</summary>
        public bool Processed { get; set; }

        /// <summary>Any error message if processing failed.</summary>
        public string? ProcessingError { get; set; }
    }
}
