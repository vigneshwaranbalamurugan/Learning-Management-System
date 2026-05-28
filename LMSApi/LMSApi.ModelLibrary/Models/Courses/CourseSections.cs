namespace LMSApi.ModelLibrary.Models
{
    public class CourseSection
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int SectionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TimeLimitMinutes { get; set; }
        public int TotalMarks { get; set; }

        public int PassingMarks { get; set; }
        public int MaxAttempts { get; set; }

        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public Courses Course { get; set; }
    }
}