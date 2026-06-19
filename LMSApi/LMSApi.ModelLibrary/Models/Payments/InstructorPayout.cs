using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    /// <summary>
    /// Tracks every Razorpay Route transfer attempt to an instructor.
    /// One record per payment-to-instructor transfer (trf_xxx).
    /// </summary>
    public class InstructorPayout
    {
        public int Id { get; set; }

        /// <summary>The student payment that triggered this Route transfer.</summary>
        public int PaymentId { get; set; }

        public int InstructorId { get; set; }
        public int InstructorPayoutAccountId { get; set; }

        /// <summary>INR amount transferred to instructor (course price minus platform fee).</summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Razorpay Route Transfer ID (trf_xxx) returned after POST /payments/{id}/transfers.
        /// Null until the transfer is initiated.
        /// </summary>
        public string? RazorpayPayoutId { get; set; }

        /// <summary>Razorpay Linked Account ID (acc_xxx) used for this transfer.</summary>
        public string? RazorpayFundAccountId { get; set; }

        public PayoutStatus Status { get; set; } = PayoutStatus.Pending;

        /// <summary>Failure reason from Razorpay webhook if transfer fails/reverses.</summary>
        public string? FailureReason { get; set; }

        /// <summary>Notes for manual review (e.g. if payout fails and needs manual action).</summary>
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public Payments Payment { get; set; } = null!;
        public Users Instructor { get; set; } = null!;
        public InstructorPayoutAccount PayoutAccount { get; set; } = null!;
    }
}
