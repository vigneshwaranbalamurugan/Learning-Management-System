namespace LMSApi.ModelLibrary.Models
{
    public class Assignments
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int LessonId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TotalMarks { get; set; }
        public string AttachmentUrl { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation property
        public Courses Course { get; set; }
    }
}