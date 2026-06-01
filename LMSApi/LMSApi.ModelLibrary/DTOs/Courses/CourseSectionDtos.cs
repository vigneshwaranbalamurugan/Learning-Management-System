using System.ComponentModel.DataAnnotations;

namespace LMSApi.ModelLibrary.DTOs
{
    // ─── Requests ────────────────────────────────────────────────────────────

    public class CreateSectionRequest
    {
        [Required(ErrorMessage = "Course ID is required.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Section title is required.")]
        [MaxLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string Title { get; set; }

        [MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        public string? Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Time limit must be zero or greater.")]
        public int TimeLimitMinutes { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Total marks must be zero or greater.")]
        public int TotalMarks { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Passing marks must be zero or greater.")]
        public int PassingMarks { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max attempts must be at least 1.")]
        public int MaxAttempts { get; set; } = 1;
    }

    public class UpdateSectionRequest
    {
        [MaxLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string? Title { get; set; }

        [MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        public string? Description { get; set; }

        [Range(0, int.MaxValue)]
        public int? TimeLimitMinutes { get; set; }

        [Range(0, int.MaxValue)]
        public int? TotalMarks { get; set; }

        [Range(0, int.MaxValue)]
        public int? PassingMarks { get; set; }

        [Range(1, int.MaxValue)]
        public int? MaxAttempts { get; set; }

        public bool? IsPublished { get; set; }
    }

    // ─── Responses ───────────────────────────────────────────────────────────

    public class SectionResponse
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int TimeLimitMinutes { get; set; }
        public int TotalMarks { get; set; }
        public int PassingMarks { get; set; }
        public int MaxAttempts { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
