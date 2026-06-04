using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    /// <summary>
    /// Represents a fixed cohort (batch) within a CohortBased course.
    /// One course can have multiple batches running at different times.
    /// </summary>
    public class CourseBatch
    {
        public int Id { get; set; }
        public int CourseId { get; set; }

        /// <summary>Human-readable batch name, e.g. "Batch 2026 — July Cohort".</summary>
        public string Name { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>Window during which students can enroll into this batch.</summary>
        public DateTime EnrollmentStartDate { get; set; }
        public DateTime EnrollmentEndDate { get; set; }

        /// <summary>Maximum number of students allowed in this batch.</summary>
        public int MaxStudents { get; set; }

        public BatchStatus Status { get; set; } = BatchStatus.Upcoming;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ─── Not mapped: populated by PostgreSQL function ─────────────────────
        /// <summary>
        /// Available seats = MaxStudents - enrolled count.
        /// Populated by calling the PostgreSQL function <c>get_batch_available_seats(batch_id)</c>.
        /// Not persisted in the database.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int AvailableSeats { get; set; }

        // ─── Navigation properties ────────────────────────────────────────────
        public Courses Course { get; set; } = null!;
        public ICollection<Enrollments> Enrollments { get; set; } = new List<Enrollments>();
    }
}
