using LMSApi.ModelLibrary.Enums;
using System.ComponentModel.DataAnnotations;

namespace LMSApi.ModelLibrary.DTOs
{
    // ─── Requests ────────────────────────────────────────────────────────────

    public class PublishSectionRequest
    {
        public bool Publish { get; set; }
    }

    public class CreateSectionRequest
    {
        [Required(ErrorMessage = "Course ID is required.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Section title is required.")]
        [MaxLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string Title { get; set; }

        [MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        public string? Description { get; set; }



        [Range(0, int.MaxValue, ErrorMessage = "Sort order must be zero or greater.")]
        public int SortOrder { get; set; }
    }

    public class UpdateSectionRequest
    {
        [MaxLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string? Title { get; set; }

        [MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        public string? Description { get; set; }



        [Range(0, int.MaxValue)]
        public int? SortOrder { get; set; }

        public PublishStatus? Status { get; set; }
    }

    public class ReorderSectionsRequest
    {
        [Required]
        public List<SectionOrderItem> SectionOrders { get; set; } = [];
    }

    public class SectionOrderItem
    {
        public int SectionId { get; set; }
        public int SortOrder { get; set; }
    }

    // ─── Responses ───────────────────────────────────────────────────────────

    public class SectionResponse
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public int SortOrder { get; set; }
        public PublishStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public IEnumerable<LessonResponse> Lessons { get; set; } = [];
        public IEnumerable<QuizResponse> Quizzes { get; set; } = [];
        public IEnumerable<AssignmentResponse> Assignments { get; set; } = [];
    }

    public class CourseSectionPreviewResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public int SortOrder { get; set; }
        public IEnumerable<CourseLessonPreviewResponse> Lessons { get; set; } = [];
        public IEnumerable<QuizResponse> Quizzes { get; set; } = [];
        public IEnumerable<AssignmentResponse> Assignments { get; set; } = [];
    }
}
