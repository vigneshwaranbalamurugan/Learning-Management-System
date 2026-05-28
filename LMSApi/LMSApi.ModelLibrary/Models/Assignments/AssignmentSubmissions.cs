namespace LMSApi.ModelLibrary.Models
{
    public class AssignmentSubmissions
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public string SubmissionText { get; set; }
        public string SubmittedFileUrl { get; set; }
        public int? MarksAwarded { get; set; }
        public string Feedback { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? GradedAt { get; set; }
        
        // Navigation properties
        public Assignments Assignment { get; set; }
        public Users Student { get; set; }
    }
}