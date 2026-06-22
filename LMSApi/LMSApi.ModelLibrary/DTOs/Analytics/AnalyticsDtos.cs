namespace LMSApi.ModelLibrary.DTOs
{
    public class AdminAnalyticsResponse
    {
        public int TotalUsers { get; set; }
        public int TotalLearners { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalCourses { get; set; }
        public int ActiveCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class RecentEnrollmentDto
    {
        public string StudentName { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
    }

    public class InstructorAnalyticsResponse
    {
        public int TotalCoursesCreated { get; set; }
        public int TotalStudentsEnrolled { get; set; }
        public decimal TotalRevenueGenerated { get; set; }
        public decimal? AverageQuizScore { get; set; }
        public decimal? AverageAssignmentScore { get; set; }
        public System.Collections.Generic.List<RecentEnrollmentDto> RecentEnrollments { get; set; } = new();
    }

    public class LearnerAnalyticsResponse
    {
        public int TotalEnrolledCourses { get; set; }
        public int CompletedCourses { get; set; }
        public int InProgressCourses { get; set; }
        public decimal AverageProgressPercentage { get; set; }
        public decimal? AverageQuizScore { get; set; }
        public decimal? AverageAssignmentScore { get; set; }
    }
}
