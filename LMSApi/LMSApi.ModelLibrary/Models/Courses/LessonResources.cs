using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    public class LessonResources
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public ResourceType ResourceType { get; set; } // e.g., Video, Document, Link
        public string ResourceTitle { get; set; }
        public string ResourceUrl { get; set; }
        public string Description { get; set; }
        public DateTime UploadedAt { get; set; }

        // Navigation property
        public Lessons Lesson { get; set; }
    }
}