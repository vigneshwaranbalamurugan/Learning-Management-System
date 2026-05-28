namespace LMSApi.ModelLibrary.Models
{
    public class Reviews
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; } // e.g., 1 to 5
        public string Review { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Courses Course { get; set; }
        public Users User { get; set; }
    }
}