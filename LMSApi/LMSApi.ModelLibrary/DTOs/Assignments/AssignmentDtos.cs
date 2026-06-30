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

        public AssignmentAttachmentType AttachmentType { get; set; } = AssignmentAttachmentType.None;

        public string? AttachmentUrl { get; set; }

        /// <summary>Days from enrollment date. 0 = no deadline.</summary>
        [Range(0, int.MaxValue)]
        public int DeadlineInDays { get; set; }

        public DateTime? DeadlineDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "MaxSubmissions must be at least 1.")]
        public int MaxSubmissions { get; set; } = 1;

        public bool IsLateSubmissionAllowed { get; set; }

        [Range(0, int.MaxValue)]
        public int SortOrder { get; set; }
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

        public AssignmentAttachmentType? AttachmentType { get; set; }

        public string? AttachmentUrl { get; set; }

        [Range(0, int.MaxValue)]
        public int? DeadlineInDays { get; set; }

        public DateTime? DeadlineDate { get; set; }

        [Range(1, int.MaxValue)]
        public int? MaxSubmissions { get; set; }

        public bool? IsLateSubmissionAllowed { get; set; }

        [Range(0, int.MaxValue)]
        public int? SortOrder { get; set; }

        public PublishStatus? Status { get; set; }
    }

    // ─── Assignment Responses ────────────────────────────────────────────────

    public class PublishAssignmentRequest
    {
        public bool Publish { get; set; }
    }

    public class ReorderAssignmentsRequest
    {
        [Required]
        public List<AssignmentOrderItem> AssignmentOrders { get; set; } = [];
    }

    public class AssignmentOrderItem
    {
        public int AssignmentId { get; set; }
        public int SortOrder { get; set; }
    }

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
        public AssignmentAttachmentType AttachmentType { get; set; }
        public string? AttachmentUrl { get; set; }
        public int DeadlineInDays { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public int MaxSubmissions { get; set; }
        public bool IsLateSubmissionAllowed { get; set; }
        public int SortOrder { get; set; }
        public PublishStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class InstructorAssignmentSummaryDto
    {
        public int Id { get; set; }
        public int CourseSectionId { get; set; }
        public string Title { get; set; }
        public string CourseTitle { get; set; }
        public string SectionTitle { get; set; }
        public int TotalMarks { get; set; }
        public int DeadlineInDays { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public int PendingSubmissionsCount { get; set; }
        public PublishStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ─── Submission Requests ─────────────────────────────────────────────────

    public class AssignmentSubmissionRequest
    {
        [Required]
        public int AssignmentId { get; set; }

        public string? SubmissionText { get; set; }

        public AssignmentSubmissonAttachmentType? AttachmentType { get; set; }

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
        public AssignmentSubmissonAttachmentType? AttachmentType { get; set; }
        public string? SubmittedAssignmentUrl { get; set; }
        public int? MarksAwarded { get; set; }
        public string? Feedback { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? GradedAt { get; set; }
        public string Status { get; set; }
        public bool? IsPassed { get; set; }
        public int AttemptNumber { get; set; }
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }
        public bool IsLate { get; set; }
        public DateTime? StudentDeadline { get; set; }
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

    public class PagedAssignmentSubmissionResponse
    {
        public IEnumerable<AssignmentSubmissionResponse> Submissions { get; set; } = new List<AssignmentSubmissionResponse>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
