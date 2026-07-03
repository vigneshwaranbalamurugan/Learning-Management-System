namespace LMSApi.ModelLibrary.DTOs
{
    public class MonthlyRevenueDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class MonthlyTrendDto
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class RecentActivityDto
    {
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class AdminAnalyticsResponse
    {
        public int TotalUsers { get; set; }
        public int TotalLearners { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalCourses { get; set; }
        public int ActiveCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCertificatesIssued { get; set; }
        public System.Collections.Generic.List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
        public System.Collections.Generic.List<MonthlyTrendDto> UserGrowth { get; set; } = new();
        public System.Collections.Generic.List<MonthlyTrendDto> EnrollmentTrend { get; set; } = new();
        public System.Collections.Generic.List<RecentActivityDto> RecentActivities { get; set; } = new();
        public System.Collections.Generic.List<CourseListItemResponse> RecentCourses { get; set; } = new();
        public System.Collections.Generic.List<UserMetadataDto> RecentUsers { get; set; } = new();
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
        public System.Collections.Generic.List<InstructorCourseCardResponse> RecentCourses { get; set; } = new();
    }

    public class LearnerAnalyticsResponse
    {
        public int TotalEnrolledCourses { get; set; }
        public int CompletedCourses { get; set; }
        public int InProgressCourses { get; set; }
        public decimal AverageProgressPercentage { get; set; }
        public decimal? AverageQuizScore { get; set; }
        public decimal? AverageAssignmentScore { get; set; }
        public System.Collections.Generic.List<EnrollmentResponse> MyCourses { get; set; } = new();
    }

    public class UserMetadataDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
