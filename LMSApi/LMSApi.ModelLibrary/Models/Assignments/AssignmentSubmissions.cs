using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    public class AssignmentSubmissions
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

        // New fields
        public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
        public bool? IsPassed { get; set; }
        public int AttemptNumber { get; set; } = 1;

        // Navigation properties
        public Assignments Assignment { get; set; }
        public Users Student { get; set; }
    }
}