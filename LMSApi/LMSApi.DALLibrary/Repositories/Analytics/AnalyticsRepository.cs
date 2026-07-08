using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LMSApi.DALLibrary.Repositories
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly LMSDbContext _context;

        public AnalyticsRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task<AdminAnalyticsResponse> GetAdminAnalyticsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalLearners = await _context.Users.CountAsync(u => u.Role.RoleName == "Learner");
            var totalInstructors = await _context.Users.CountAsync(u => u.Role.RoleName == "Instructor");
            var totalCourses = await _context.Courses.CountAsync();
            var activeCourses = await _context.Courses.CountAsync(c => c.Status == CourseStatus.Published);
            var totalEnrollments = await _context.Enrollments.CountAsync(e => e.EnrollmentStatus == EnrollmentStatus.Active || e.EnrollmentStatus == EnrollmentStatus.Completed);
            
            var totalRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed || p.Status == PaymentStatus.Transferred)
                .SumAsync(p => p.Amount);

            var totalCertificatesIssued = await _context.Certificates.CountAsync();

            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

            // 1. Monthly Revenue
            var monthlyRevenueData = await _context.Payments
                .Where(p => (p.Status == PaymentStatus.Completed || p.Status == PaymentStatus.Transferred) && p.PaidAt != null && p.PaidAt >= sixMonthsAgo)
                .GroupBy(p => new { Year = p.PaidAt.Value.Year, Month = p.PaidAt.Value.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Revenue = g.Sum(p => p.Amount)
                })
                .ToListAsync();

            var monthlyRevenueList = new System.Collections.Generic.List<MonthlyRevenueDto>();
            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.UtcNow.AddMonths(-i);
                var monthData = monthlyRevenueData.FirstOrDefault(m => m.Year == targetDate.Year && m.Month == targetDate.Month);
                monthlyRevenueList.Add(new MonthlyRevenueDto
                {
                    Month = targetDate.ToString("MMM"),
                    Revenue = monthData?.Revenue ?? 0
                });
            }

            // 2. User Growth Trend
            var userGrowthData = await _context.Users
                .Where(u => u.CreatedAt >= sixMonthsAgo)
                .GroupBy(u => new { Year = u.CreatedAt.Year, Month = u.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            var userGrowthList = new System.Collections.Generic.List<MonthlyTrendDto>();
            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.UtcNow.AddMonths(-i);
                var trendData = userGrowthData.FirstOrDefault(m => m.Year == targetDate.Year && m.Month == targetDate.Month);
                userGrowthList.Add(new MonthlyTrendDto
                {
                    Month = targetDate.ToString("MMM"),
                    Count = trendData?.Count ?? 0
                });
            }

            // 3. Course Enrollment Trend
            var enrollmentTrendData = await _context.Enrollments
                .Where(e => e.EnrolledAt >= sixMonthsAgo && (e.EnrollmentStatus == EnrollmentStatus.Active || e.EnrollmentStatus == EnrollmentStatus.Completed))
                .GroupBy(e => new { Year = e.EnrolledAt.Year, Month = e.EnrolledAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            var enrollmentTrendList = new System.Collections.Generic.List<MonthlyTrendDto>();
            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.UtcNow.AddMonths(-i);
                var trendData = enrollmentTrendData.FirstOrDefault(m => m.Year == targetDate.Year && m.Month == targetDate.Month);
                enrollmentTrendList.Add(new MonthlyTrendDto
                {
                    Month = targetDate.ToString("MMM"),
                    Count = trendData?.Count ?? 0
                });
            }

            // 4. Recent Activities (Removed, fetching in separate paginated endpoint)

            return new AdminAnalyticsResponse
            {
                TotalUsers = totalUsers,
                TotalLearners = totalLearners,
                TotalInstructors = totalInstructors,
                TotalCourses = totalCourses,
                ActiveCourses = activeCourses,
                TotalEnrollments = totalEnrollments,
                TotalRevenue = totalRevenue,
                TotalCertificatesIssued = totalCertificatesIssued,
                MonthlyRevenue = monthlyRevenueList,
                UserGrowth = userGrowthList,
                EnrollmentTrend = enrollmentTrendList
            };
        }

        public async Task<System.Collections.Generic.List<RecentActivityDto>> GetAdminRecentActivitiesAsync(int pageNumber, int pageSize)
        {
            int maxTotalActivities = 30;
            int skipCount = (pageNumber - 1) * pageSize;
            
            if (skipCount >= maxTotalActivities) return new System.Collections.Generic.List<RecentActivityDto>();

            int takeCount = Math.Min(pageSize, maxTotalActivities - skipCount);

            var targetTypes = new System.Collections.Generic.List<ActivityType> {
                ActivityType.UserRegister,
                ActivityType.CourseCreated,
                ActivityType.CoursePublished,
                ActivityType.CertificateIssued
            };

            var recentActivities = await _context.ActivityLogs
                .Where(al => targetTypes.Contains(al.ActivityType))
                .OrderByDescending(al => al.Timestamp)
                .Skip(skipCount)
                .Take(takeCount)
                .Select(al => new RecentActivityDto
                {
                    ActivityType = al.ActivityType.ToString(),
                    Description = al.Description,
                    Timestamp = al.Timestamp,
                    UserName = al.User.UserProfile != null ? (al.User.UserProfile.FirstName + " " + al.User.UserProfile.LastName) : al.User.Email
                })
                .ToListAsync();

            return recentActivities;
        }

        public async Task<InstructorAnalyticsResponse> GetInstructorAnalyticsAsync(int instructorId)
        {
            var totalCoursesCreated = await _context.Courses.CountAsync(c => c.InstructorId == instructorId);
            
            var courseIds = await _context.Courses
                .Where(c => c.InstructorId == instructorId)
                .Select(c => c.Id)
                .ToListAsync();

            var totalStudentsEnrolled = await _context.Enrollments
                .CountAsync(e => courseIds.Contains(e.CourseId) && (e.EnrollmentStatus == EnrollmentStatus.Active || e.EnrollmentStatus == EnrollmentStatus.Completed));

            var totalRevenueGenerated = await _context.Payments
                .Where(p => courseIds.Contains(p.CourseId) 
                    && (p.Status == PaymentStatus.Completed || p.Status == PaymentStatus.Transferred))
                .SumAsync(p => p.Amount);

            // Calculate averages from Quiz Attempts and Assignment Submissions related to the instructor's courses
            var quizAttempts = await _context.QuizAttempts
                .Where(qa => courseIds.Contains(qa.Quiz.CourseSection.CourseId) && qa.Status == AttemptStatus.Submitted)
                .ToListAsync();

            decimal? avgQuizScore = null;
            if (quizAttempts.Any())
            {
                avgQuizScore = (decimal)quizAttempts.Average(qa => qa.Score);
            }

            var assignmentSubmissions = await _context.AssignmentSubmissions
                .Where(sub => courseIds.Contains(sub.Assignment.CourseSection.CourseId) && sub.Status == SubmissionStatus.Graded)
                .ToListAsync();

            decimal? avgAssignmentScore = null;
            if (assignmentSubmissions.Any())
            {
                avgAssignmentScore = (decimal?)assignmentSubmissions.Average(sub => sub.MarksAwarded);
            }

            var recentEnrollments = await _context.Enrollments
                .Where(e => courseIds.Contains(e.CourseId) && (e.EnrollmentStatus == EnrollmentStatus.Active || e.EnrollmentStatus == EnrollmentStatus.Completed))
                .OrderByDescending(e => e.EnrolledAt)
                .Take(5)
                .Select(e => new RecentEnrollmentDto
                {
                    StudentName = e.User.UserProfile != null ? (e.User.UserProfile.FirstName + " " + e.User.UserProfile.LastName) : e.User.Email,
                    CourseTitle = e.Course.Title,
                    EnrolledAt = e.EnrolledAt
                })
                .ToListAsync();

            return new InstructorAnalyticsResponse
            {
                TotalCoursesCreated = totalCoursesCreated,
                TotalStudentsEnrolled = totalStudentsEnrolled,
                TotalRevenueGenerated = totalRevenueGenerated,
                AverageQuizScore = avgQuizScore,
                AverageAssignmentScore = avgAssignmentScore,
                RecentEnrollments = recentEnrollments
            };
        }

        public async Task<LearnerAnalyticsResponse> GetLearnerAnalyticsAsync(int learnerId)
        {
            var enrollments = await _context.Enrollments
                .Where(e => e.UserId == learnerId && (e.EnrollmentStatus == EnrollmentStatus.Active || e.EnrollmentStatus == EnrollmentStatus.Completed))
                .ToListAsync();

            var totalEnrolledCourses = enrollments.Count;
            var completedCourses = enrollments.Count(e => e.IsCompleted);
            var inProgressCourses = enrollments.Count(e => !e.IsCompleted);
            
            decimal avgProgress = 0;
            if (enrollments.Any())
            {
                avgProgress = enrollments.Average(e => e.ProgressPercentage);
            }

            var quizAttempts = await _context.QuizAttempts
                .Where(qa => qa.UserId == learnerId && qa.Status == AttemptStatus.Submitted)
                .ToListAsync();

            decimal? avgQuizScore = null;
            if (quizAttempts.Any())
            {
                avgQuizScore = (decimal)quizAttempts.Average(qa => qa.Score);
            }

            var assignmentSubmissions = await _context.AssignmentSubmissions
                .Where(sub => sub.StudentId == learnerId && sub.Status == SubmissionStatus.Graded)
                .ToListAsync();

            decimal? avgAssignmentScore = null;
            if (assignmentSubmissions.Any())
            {
                avgAssignmentScore = (decimal?)assignmentSubmissions.Average(sub => sub.MarksAwarded);
            }

            return new LearnerAnalyticsResponse
            {
                TotalEnrolledCourses = totalEnrolledCourses,
                CompletedCourses = completedCourses,
                InProgressCourses = inProgressCourses,
                AverageProgressPercentage = avgProgress,
                AverageQuizScore = avgQuizScore,
                AverageAssignmentScore = avgAssignmentScore
            };
        }
    }
}
