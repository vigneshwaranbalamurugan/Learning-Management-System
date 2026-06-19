using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    public class Payments
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public int? BatchId { get; set; }
        public int? EnrollmentId { get; set; }
        public string ProviderOrderId { get; set; } = string.Empty;
        public string? ProviderPaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
        public string? RawResponse { get; set; }

        // ── Platform Fee Snapshot (immutable after payment) ───────────────────
        /// <summary>The fee config used at the time of this payment (null for free courses).</summary>
        public int? PlatformFeeConfigId { get; set; }

        /// <summary>The actual INR amount retained as platform fee for this payment.</summary>
        public decimal PlatformFeeAmount { get; set; } = 0;

        /// <summary>The actual INR amount to be paid out to the instructor.</summary>
        public decimal InstructorAmount { get; set; } = 0;

        /// <summary>Snapshot of the fee type used (Percentage / Flat) at payment time.</summary>
        public FeeType? FeeTypeSnapshot { get; set; }

        /// <summary>Snapshot of the fee value (% or INR) used at payment time.</summary>
        public decimal? FeeValueSnapshot { get; set; }

        // Navigation properties
        public Users User { get; set; } = null!;
        public Courses Course { get; set; } = null!;
        public Enrollments Enrollment { get; set; } = null!;
        public PlatformFeeConfig? PlatformFeeConfig { get; set; }
        public ICollection<InstructorPayout> InstructorPayouts { get; set; } = new List<InstructorPayout>();
    }
}