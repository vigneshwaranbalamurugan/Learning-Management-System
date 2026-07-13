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
        public int? BatchId { get; set; }
        public DateTime? AccessExpiresAt { get; set; }
        public EnrollmentStatus EnrollmentStatus { get; set; } = EnrollmentStatus.Active;
        public bool IsOnLatestVersion { get; set; } = true;

        // ─── Navigation properties ────────────────────────────────────────────
        public Users User { get; set; } = null!;
        public Courses Course { get; set; } = null!;
        public CourseBatch? Batch { get; set; }
        public Payments? Payment { get; set; }
    }
}