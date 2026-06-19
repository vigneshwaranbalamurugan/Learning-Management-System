using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IInstructorPayoutService
    {
        // ── Instructor: Payout Account Registration ────────────────────────────
        /// <summary>Instructor submits their details for Route Linked Account creation.</summary>
        Task<InstructorPayoutAccount> RegisterPayoutAccountAsync(
            int instructorId,
            RegisterPayoutAccountRequest request);

        /// <summary>Instructor updates their existing Route Linked Account and local record.</summary>
        Task<InstructorPayoutAccount> UpdatePayoutAccountAsync(
            int instructorId,
            RegisterPayoutAccountRequest request);

        /// <summary>Get the active payout account for an instructor.</summary>
        Task<InstructorPayoutAccount?> GetActiveAccountAsync(int instructorId);

        // ── Route Transfer (Payout) ────────────────────────────────────────────
        /// <summary>
        /// Initiate a Razorpay Route transfer to instructor's linked account.
        /// Called after successful payment capture.
        /// </summary>
        Task<InstructorPayout> InitiatePayoutAsync(Payments payment, int instructorId);

        /// <summary>
        /// Process a Razorpay webhook event (transfer.processed/failed/reversed).
        /// Updates payout status; sets PendingManualReview on failure so funds are never silently lost.
        /// </summary>
        Task HandleWebhookAsync(string razorpayTransferId, string eventType, string? failureReason = null);

        // ── Revenue Reporting ──────────────────────────────────────────────────
        Task<IEnumerable<InstructorPayout>> GetPayoutsForInstructorAsync(int instructorId);
        Task<IEnumerable<InstructorPayout>> GetAllPayoutsAsync();
        Task<IEnumerable<InstructorPayout>> GetPendingManualReviewAsync();
    }
}
