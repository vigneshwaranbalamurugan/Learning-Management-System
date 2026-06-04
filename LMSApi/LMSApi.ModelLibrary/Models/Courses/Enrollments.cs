using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    public class Enrollments
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrolledAt { get; set; }
        public decimal ProgressPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }

        // ─── Hybrid Learning fields ───────────────────────────────────────────
        /// <summary>
        /// Null for SelfPaced enrollments; required for CohortBased enrollments.
        /// </summary>
        public int? BatchId { get; set; }

        /// <summary>
        /// When access to the course expires.
        /// SelfPaced: auto-calculated as EnrolledAt + DefaultAssignmentDeadlineDays (null = never).
        /// CohortBased: mirrors the batch EndDate.
        /// </summary>
        public DateTime? AccessExpiresAt { get; set; }

        /// <summary>Current lifecycle status of this enrollment.</summary>
        public EnrollmentStatus EnrollmentStatus { get; set; } = EnrollmentStatus.Active;

        // ─── Navigation properties ────────────────────────────────────────────
        public Users User { get; set; } = null!;
        public Courses Course { get; set; } = null!;
        public CourseBatch? Batch { get; set; }
    }
}