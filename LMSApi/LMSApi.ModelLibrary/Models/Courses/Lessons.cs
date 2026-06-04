using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    public class Lessons
    {
        public int Id { get; set; }
        public int CourseSectionId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }

        /// <summary>Content field for Article-type lessons (HTML/Markdown).</summary>
        public string? Content { get; set; }

        /// <summary>
        /// URL of the primary lesson content.
        /// Video   → hosted video URL
        /// Pdf     → uploaded PDF URL
        /// ExternalLink → external URL
        /// Article → null (content lives in <see cref="Content"/>)
        /// </summary>
        public string? ContentUrl { get; set; }

        public LessonType Type { get; set; }
        public TimeSpan? DurationInMinutes { get; set; }
        public int SortOrder { get; set; }

        public bool IsPreview { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public CourseSection CourseSection { get; set; }
        public ICollection<LessonResources> Resources { get; set; } = new List<LessonResources>();
    }
}