namespace LMSApi.BALLibrary.Interfaces
{
    /// <summary>
    /// Handles cross-cutting Razorpay webhook events that are not tied to a single
    /// existing service: settlement, product route, and payment downtime events.
    /// </summary>
    public interface IWebhookEventService
    {
        /// <summary>
        /// Handles settlement.processed — logs the event and notifies all admin users
        /// via email + real-time SignalR notification.
        /// </summary>
        Task HandleSettlementAsync(string settlementId, string rawPayload);

        /// <summary>
        /// Handles product.route.* events — updates the InstructorPayoutProduct status
        /// and notifies the instructor via email + real-time notification.
        /// </summary>
        Task HandleProductRouteAsync(string razorpayAccountId, string eventType);

        /// <summary>
        /// Handles payment.downtime.* events — logs the event only (per user decision).
        /// No user notifications are sent for downtime events.
        /// </summary>
        Task HandlePaymentDowntimeAsync(string eventType, string rawPayload);
    }
}
