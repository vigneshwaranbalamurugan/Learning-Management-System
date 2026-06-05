using System.ComponentModel.DataAnnotations;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.DTOs
{
    // ─── Assignment Requests ─────────────────────────────────────────────────

    public class CreateAssignmentRequest
    {
        [Required]
        public int CourseSectionId { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(5000)]
        public string? Instructions { get; set; }

        public bool IsCompulsory { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "TotalMarks must be greater than 0.")]
        public int TotalMarks { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "PassingMarks must be >= 0.")]
        public int PassingMarks { get; set; }

        public string? AttachmentUrl { get; set; }

        /// <summary>Days from enrollment date. 0 = no deadline.</summary>
        [Range(0, int.MaxValue)]
        public int DurationLimitInDays { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "MaxSubmissions must be at least 1.")]
        public int MaxSubmissions { get; set; } = 1;

        public bool IsLateSubmissionAllowed { get; set; }
    }

    public class UpdateAssignmentRequest
    {
        [MaxLength(300)]
        public string? Title { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(5000)]
        public string? Instructions { get; set; }

        public bool? IsCompulsory { get; set; }

        [Range(1, int.MaxValue)]
        public int? TotalMarks { get; set; }

        [Range(0, int.MaxValue)]
        public int? PassingMarks { get; set; }

        public string? AttachmentUrl { get; set; }

        [Range(0, int.MaxValue)]
        public int? DurationLimitInDays { get; set; }

        [Range(1, int.MaxValue)]
        public int? MaxSubmissions { get; set; }

        public bool? IsLateSubmissionAllowed { get; set; }
    }

    // ─── Assignment Responses ────────────────────────────────────────────────

    public class AssignmentResponse
    {
        public int Id { get; set; }
        public int CourseSectionId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Instructions { get; set; }
        public bool IsCompulsory { get; set; }
        public int TotalMarks { get; set; }
        public int PassingMarks { get; set; }
        public string? AttachmentUrl { get; set; }
        public int DurationLimitInDays { get; set; }
        public int MaxSubmissions { get; set; }
        public bool IsLateSubmissionAllowed { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ─── Submission Requests ─────────────────────────────────────────────────

    public class AssignmentSubmissionRequest
    {
        [Required]
        public int AssignmentId { get; set; }

        public string? SubmissionText { get; set; }

        public string? SubmittedAssignmentUrl { get; set; }
    }

    public class GradeSubmissionRequest
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "MarksAwarded must be >= 0.")]
        public int MarksAwarded { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Feedback is required when grading.")]
        public string Feedback { get; set; }
    }

    // ─── Submission Responses ────────────────────────────────────────────────

    public class AssignmentSubmissionResponse
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public string? SubmissionText { get; set; }
        public string? SubmittedAssignmentUrl { get; set; }
        public int? MarksAwarded { get; set; }
        public string? Feedback { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? GradedAt { get; set; }
        public string Status { get; set; }
        public bool? IsPassed { get; set; }
        public int AttemptNumber { get; set; }
    }

    public class AssignmentStatusResponse
    {
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public int AttemptsMade { get; set; }
        public int MaxSubmissions { get; set; }
        public int RemainingAttempts { get; set; }
        public bool? IsPassed { get; set; }
        public string? LatestStatus { get; set; }
        public DateTime? Deadline { get; set; }
    }
}
