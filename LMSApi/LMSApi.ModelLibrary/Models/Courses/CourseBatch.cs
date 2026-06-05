using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{

    public class CourseBatch
    {
        public int Id { get; set; }
        public int CourseId { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime EnrollmentStartDate { get; set; }
        public DateTime EnrollmentEndDate { get; set; }

        public int MaxStudents { get; set; }

        public BatchStatus Status { get; set; } = BatchStatus.Upcoming;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int AvailableSeats { get; set; }

        // ─── Navigation properties ────────────────────────────────────────────
        public Courses Course { get; set; } = null!;
        public ICollection<Enrollments> Enrollments { get; set; } = new List<Enrollments>();
    }
}
