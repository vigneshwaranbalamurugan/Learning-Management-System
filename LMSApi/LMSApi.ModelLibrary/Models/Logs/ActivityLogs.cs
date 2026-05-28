using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{

    public class ActivityLogs
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public ActivityType ActivityType { get; set; } // e.g., "CourseEnrollment", "QuizAttempt", "DiscussionPost"
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }

        // Navigation property
        public Users User { get; set; }
    }
}