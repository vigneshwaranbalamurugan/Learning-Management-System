using System.ComponentModel.DataAnnotations;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.DTOs
{
    // ─── Requests ────────────────────────────────────────────────────────────

    public class PublishLessonRequest
    {
        public bool Publish { get; set; }
    }

    public class CreateLessonRequest
    {
        [Required(ErrorMessage = "Course section ID is required.")]
        public int CourseSectionId { get; set; }

        [Required(ErrorMessage = "Lesson title is required.")]
        [MaxLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string Title { get; set; }

        [MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        public string? Description { get; set; }

        /// <summary>
        /// Required for Article type lessons (HTML or Markdown body).
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Required for Video, Pdf, and ExternalLink type lessons.
        /// </summary>
        public string? ContentUrl { get; set; }

        [Required(ErrorMessage = "Lesson type is required.")]
        public LessonType Type { get; set; }

        public TimeSpan? DurationInMinutes { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Sort order must be zero or greater.")]
        public int SortOrder { get; set; }

        public bool? IsPreview { get; set; }
        public PublishStatus? Status { get; set; }
    }

    public class UpdateLessonRequest
    {
        public int? CourseSectionId { get; set; }

        [MaxLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string? Title { get; set; }

        [MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        public string? Description { get; set; }

        public string? Content { get; set; }
        public string? ContentUrl { get; set; }
        public LessonType? Type { get; set; }
        public TimeSpan? DurationInMinutes { get; set; }

        [Range(0, int.MaxValue)]
        public int? SortOrder { get; set; }

        public bool? IsPreview { get; set; }
        public PublishStatus? Status { get; set; }
    }

    public class ReorderLessonsRequest
    {
        [Required]
        public List<LessonOrderItem> LessonOrders { get; set; } = [];
    }

    public class LessonOrderItem
    {
        public int LessonId { get; set; }
        public int SortOrder { get; set; }
    }

    // ─── Responses ───────────────────────────────────────────────────────────

    public class LessonResponse
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
        public PublishStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class LessonDetailResponse : LessonResponse
    {
        public List<ResourceResponse> Resources { get; set; } = [];
    }
}
