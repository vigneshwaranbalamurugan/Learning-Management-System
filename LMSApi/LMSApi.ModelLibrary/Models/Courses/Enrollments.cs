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
        // Navigation properties
        public Users User { get; set; }
        public Courses Course { get; set; }
    }
}