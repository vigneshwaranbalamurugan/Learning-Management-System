using LMSApi.ModelLibrary.Enums;
using System.ComponentModel.DataAnnotations.Schema;

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

        // ─── Hybrid Learning fields ───────────────────────────────────────────
        /// <summary>SelfPaced (default) or CohortBased.</summary>
        public CourseAccessType CourseAccessType { get; set; } = CourseAccessType.SelfPaced;

        /// <summary>
        /// For SelfPaced: access expires this many days after enrollment.
        /// Null means no expiry. Ignored for CohortBased (batch end date governs).
        /// </summary>
        public int? DefaultAssignmentDeadlineDays { get; set; }

        // Navigation properties
        public CourseCategories Category { get; set; }
        public Users Instructor { get; set; }
        public ICollection<CourseSection> Sections { get; set; } = new List<CourseSection>();
        public ICollection<CourseBatch> Batches { get; set; } = new List<CourseBatch>();
    }
}