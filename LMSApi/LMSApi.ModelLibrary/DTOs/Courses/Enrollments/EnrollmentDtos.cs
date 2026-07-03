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
        
        /// <summary>
        /// e.g. "Razorpay" or "Stripe"
        /// </summary>
        public string? ProviderName { get; set; }
    }

    public class VerifyPaymentRequest
    {
        [Required]
        public string ProviderName { get; set; } = string.Empty;
        [Required]
        public string ProviderOrderId { get; set; } = string.Empty;
        [Required]
        public string ProviderPaymentId { get; set; } = string.Empty;
        [Required]
        public string ProviderSignature { get; set; } = string.Empty;
        
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

        // Enriched course metadata
        public string? ThumbnailUrl { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string LanguageName { get; set; } = string.Empty;
        public CourseLevel Level { get; set; }
        public int LessonsCount { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public bool HasCertificate { get; set; } = true;
        public CourseAccessType CourseAccessType { get; set; }
    }

    public class StudentProgressSummaryDto
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
        public EnrollmentStatus EnrollmentStatus { get; set; }
        public decimal ProgressPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? BatchName { get; set; }
    }

    public class PagedEnrollmentResponse
    {
        public IEnumerable<EnrollmentResponse> Enrollments { get; set; } = [];
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
