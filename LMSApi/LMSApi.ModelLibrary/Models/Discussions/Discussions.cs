namespace LMSApi.ModelLibrary.Models
{
    public class Discussions
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsPinned { get; set; }
        public bool IsLocked { get; set; }
        public DateTime CreatedAt { get; set; }
        // Navigation properties
        public Courses Course { get; set; }
        public Users User { get; set; }
    }
}