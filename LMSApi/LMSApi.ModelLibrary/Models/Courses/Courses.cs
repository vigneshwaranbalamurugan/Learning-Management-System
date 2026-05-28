using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    public class Courses
    {
        public int Id { get; set; }
        public int InstructorId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; }
        public string slug { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public bool IsPremium { get; set; }
        
        public string ThumbnailUrl { get; set; }
        public string IntroVideoUrl { get; set; }
        public string Requirements { get; set; }
        public string LearningOutcomes { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public CourseLevel Level { get; set; }
        public CourseLanguage Language { get; set; }
        public CourseStatus Status { get; set; }
        public DateTime PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public CourseCategories Category { get; set; }
        public Users Instructor { get; set; }
    }
}