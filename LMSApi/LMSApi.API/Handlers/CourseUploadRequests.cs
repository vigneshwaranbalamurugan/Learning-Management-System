using System.ComponentModel.DataAnnotations;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.API.Handlers.Courses
{
    /// <summary>
    /// Multipart/form-data wrapper for creating a course.
    /// Contains all course fields plus optional thumbnail image and intro video files.
    /// </summary>
    public class CreateCourseFormRequest
    {
        [Required(ErrorMessage = "Category ID is required.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Course title is required.")]
        [MaxLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string Title { get; set; }

        [MaxLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be zero or greater.")]
        public decimal? Price { get; set; }

        public bool IsPremium { get; set; } = false;

        public string? Requirements { get; set; }
        public string? LearningOutcomes { get; set; }
        public TimeSpan EstimatedDuration { get; set; }

        public CourseLevel Level { get; set; } = CourseLevel.Beginner;
        public CourseLanguage Language { get; set; } = CourseLanguage.English;

        // ─── Hybrid Learning ─────────────────────────────────────────────────
        /// <summary>SelfPaced (default) or CohortBased.</summary>
        public CourseAccessType CourseAccessType { get; set; } = CourseAccessType.SelfPaced;

        /// <summary>
        /// For SelfPaced: access expires this many days after enrollment.
        /// Null means no expiry.
        /// </summary>
        public int? DefaultAssignmentDeadlineDays { get; set; }

        /// <summary>Optional course thumbnail image (JPG, JPEG, PNG).</summary>
        public IFormFile? Thumbnail { get; set; }

        /// <summary>Optional course intro video (MP4, MOV, AVI, WEBM).</summary>
        public IFormFile? IntroVideo { get; set; }
    }

    /// <summary>
    /// Multipart/form-data wrapper for updating a course.
    /// All fields are optional. Include Thumbnail/IntroVideo only when replacing the file.
    /// </summary>
    public class UpdateCourseFormRequest
    {
        [MaxLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string? Title { get; set; }

        public int? CategoryId { get; set; }

        [MaxLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be zero or greater.")]
        public decimal? Price { get; set; }

        public bool? IsPremium { get; set; }
        public string? Requirements { get; set; }
        public string? LearningOutcomes { get; set; }
        public TimeSpan? EstimatedDuration { get; set; }
        public CourseLevel? Level { get; set; }
        public CourseLanguage? Language { get; set; }

        // ─── Hybrid Learning ─────────────────────────────────────────────────
        public CourseAccessType? CourseAccessType { get; set; }
        public int? DefaultAssignmentDeadlineDays { get; set; }

        /// <summary>New thumbnail image to replace the existing one (JPG, JPEG, PNG). Leave empty to keep current.</summary>
        public IFormFile? Thumbnail { get; set; }

        /// <summary>New intro video to replace the existing one (MP4, MOV, AVI, WEBM). Leave empty to keep current.</summary>
        public IFormFile? IntroVideo { get; set; }
    }
}
