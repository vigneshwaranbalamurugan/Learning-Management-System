using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    public class Lessons
    {
        public int Id { get; set; }
        public int CourseSectionId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Content { get; set; }
        public string? ContentUrl { get; set; }

        public LessonType Type { get; set; }
        public TimeSpan? DurationInMinutes { get; set; }
        public int SortOrder { get; set; }

        public bool IsPreview { get; set; }
        public PublishStatus Status { get; set; } = PublishStatus.Draft;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public CourseSection CourseSection { get; set; }
        public ICollection<LessonResources> Resources { get; set; } = new List<LessonResources>();
        public ICollection<Discussions> Discussions { get; set; } = new List<Discussions>();
    }
}
