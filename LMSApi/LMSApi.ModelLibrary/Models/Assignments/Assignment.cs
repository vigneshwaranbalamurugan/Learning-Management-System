namespace LMSApi.ModelLibrary.Models
{
    public class Assignments
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

        /// <summary>
        /// Number of days from enrollment date within which the student must submit.
        /// 0 means no deadline.
        /// </summary>
        public int DurationLimitInDays { get; set; }

        /// <summary>Maximum number of submission attempts allowed per student.</summary>
        public int MaxSubmissions { get; set; } = 1;

        public bool IsLateSubmissionAllowed { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public CourseSection CourseSection { get; set; }
        public ICollection<AssignmentSubmissions> Submissions { get; set; } = new List<AssignmentSubmissions>();
    }
}