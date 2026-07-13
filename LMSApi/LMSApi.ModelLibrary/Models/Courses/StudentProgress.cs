namespace LMSApi.ModelLibrary.Models
{
    public class StudentProgress
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int LessonId { get; set; }
        public bool IsCompleted { get; set; }
        public decimal VideoWatchedPercentage { get; set; }
        public int LastWatchedSecond { get; set; }
        public int MaxWatchedSecond { get; set; }
        public double ProgressPercentage { get; set; }
        public DateTime LastAccessed { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsArchived { get; set; } = false;

        // Navigation properties
        public Users Student { get; set; }
        public Courses Course { get; set; }
        public Lessons Lesson { get; set; }
    }
}