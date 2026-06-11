using System.ComponentModel.DataAnnotations;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.DTOs
{
    // ─── Requests ────────────────────────────────────────────────────────────

    public class CreateCourseRequest
    {
        [Required(ErrorMessage = "Category ID is required.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Course title is required.")]
        [MaxLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage ="Description is required.")]
        [MaxLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
        public string Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be zero or greater.")]
        public decimal? Price { get; set; }

        public bool IsPremium { get; set; } = false;

        public string? Requirements { get; set; }
        public string? LearningOutcomes { get; set; }

        [Required(ErrorMessage="Estimated Duration is Required.")]
        public TimeSpan EstimatedDuration { get; set; }

        public CourseLevel Level { get; set; } = CourseLevel.Beginner;
        public CourseLanguage Language { get; set; } = CourseLanguage.English;

        // ─── Hybrid Learning ─────────────────────────────────────────────────
        public CourseAccessType CourseAccessType { get; set; } = CourseAccessType.SelfPaced;

        /// <summary>
        /// Only used when CourseAccessType = SelfPaced.
        /// Access expires this many days after enrollment. Null = never expires.
        /// </summary>
        public int? DefaultDeadlineDays { get; set; }
    }

    public class UpdateCourseRequest
    {
        [MaxLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Category ID is required.")]
        public int? CategoryId { get; set; }
        [MaxLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
        public string Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be zero or greater.")]
        public decimal? Price { get; set; }

        public bool? IsPremium { get; set; }=false;
        public string? Requirements { get; set; }
        public string? LearningOutcomes { get; set; }

        public TimeSpan EstimatedDuration { get; set; }
        public CourseLevel? Level { get; set; }=CourseLevel.Beginner;
        public CourseLanguage? Language { get; set; }=CourseLanguage.English;

        // ─── Hybrid Learning ─────────────────────────────────────────────────
        public CourseAccessType? CourseAccessType { get; set; }= Enums.CourseAccessType.SelfPaced;
        public int? DefaultDeadlineDays { get; set; }
    }

    // ─── Responses ───────────────────────────────────────────────────────────

    public class PublishCourseRequest
    {
        public bool Publish { get; set; }
    }

    public class CourseResponse
    {
        public int Id { get; set; }
        public int InstructorId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public bool IsPremium { get; set; }
        public string? ThumbnailUrl { get; set; }
        public CourseLevel Level { get; set; }
        public CourseLanguage Language { get; set; }
        public CourseStatus Status { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ─── Hybrid Learning ─────────────────────────────────────────────────
        public CourseAccessType CourseAccessType { get; set; }
    }

    public class CourseDetailsResponse : CourseResponse
    {
        public string? IntroVideoUrl { get; set; }
        public string? Requirements { get; set; }
        public string? LearningOutcomes { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public IEnumerable<SectionResponse> Sections { get; set; } = [];

        // ─── Hybrid Learning ─────────────────────────────────────────────────
        /// <summary>Populated for CohortBased courses; empty list for SelfPaced.</summary>
        public IEnumerable<BatchSummaryResponse> AvailableBatches { get; set; } = [];
    }
}
