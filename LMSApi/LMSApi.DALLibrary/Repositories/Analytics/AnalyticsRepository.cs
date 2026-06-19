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

            return new AdminAnalyticsResponse
            {
                TotalUsers = totalUsers,
                TotalLearners = totalLearners,
                TotalInstructors = totalInstructors,
                TotalCourses = totalCourses,
                ActiveCourses = activeCourses,
                TotalEnrollments = totalEnrollments,
                TotalRevenue = totalRevenue
            };
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

            return new InstructorAnalyticsResponse
            {
                TotalCoursesCreated = totalCoursesCreated,
                TotalStudentsEnrolled = totalStudentsEnrolled,
                TotalRevenueGenerated = totalRevenueGenerated,
                AverageQuizScore = avgQuizScore,
                AverageAssignmentScore = avgAssignmentScore
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
