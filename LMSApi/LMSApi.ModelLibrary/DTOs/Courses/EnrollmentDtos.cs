using System.ComponentModel.DataAnnotations;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.DTOs
{
    // ─── Requests ────────────────────────────────────────────────────────────

    public class EnrollRequest
    {
        /// <summary>
        /// Required for CohortBased courses. Must be null for SelfPaced courses.
        /// </summary>
        public int? BatchId { get; set; }
    }

    public class VerifyPaymentRequest
    {
        [Required]
        public string RazorpayOrderId { get; set; }
        [Required]
        public string RazorpayPaymentId { get; set; }
        [Required]
        public string RazorpaySignature { get; set; }
        
        public int? BatchId { get; set; }
    }

    // ─── Responses ───────────────────────────────────────────────────────────

    public class EnrollmentResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int? BatchId { get; set; }

        /// <summary>Batch name when enrolled in a CohortBased course; null for SelfPaced.</summary>
        public string? BatchName { get; set; }

        public DateTime EnrolledAt { get; set; }

        /// <summary>
        /// When access expires. Null means no expiry (SelfPaced with no deadline set).
        /// </summary>
        public DateTime? AccessExpiresAt { get; set; }

        public EnrollmentStatus EnrollmentStatus { get; set; }
        public decimal ProgressPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
