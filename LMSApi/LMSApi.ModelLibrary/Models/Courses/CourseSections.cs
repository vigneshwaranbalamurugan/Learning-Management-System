namespace LMSApi.ModelLibrary.Models
{
    public class CourseSection
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int SectionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublished { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Courses Course { get; set; }
        public ICollection<Lessons> Lessons { get; set; } = new List<Lessons>();
    }
}