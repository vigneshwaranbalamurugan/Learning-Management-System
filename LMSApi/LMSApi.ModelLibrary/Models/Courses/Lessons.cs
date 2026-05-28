using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    public class Lessons
    {
        public int Id { get; set; }
        public int CourseSectionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string? ExternalUrl { get; set; }
        public TimeSpan? DurationInMinutes { get; set; }
        public LessonType Type { get; set; }
        public string VideoUrl { get; set; }
        public TimeSpan Duration { get; set; }
        public int SortOrder { get; set; }

        // Navigation property
        public CourseSection CourseSection { get; set; }
    }
}