namespace LMSApi.ModelLibrary.Models
{
   public class Quzzes
    {
        public int Id { get; set; }
        public int CourseSectionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public int Order { get; set; }

        // Navigation property
        public CourseSection CourseSection { get; set; }
    }
}