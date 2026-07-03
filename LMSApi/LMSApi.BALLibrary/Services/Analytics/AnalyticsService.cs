using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using System.Threading.Tasks;
using System.Linq;

namespace LMSApi.BALLibrary.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _analyticsRepository;
        private readonly ICourseService _courseService;
        private readonly IEnrollmentService _enrollmentService;

        public AnalyticsService(
            IAnalyticsRepository analyticsRepository,
            ICourseService courseService,
            IEnrollmentService enrollmentService)
        {
            _analyticsRepository = analyticsRepository;
            _courseService = courseService;
            _enrollmentService = enrollmentService;
        }

        public async Task<AdminAnalyticsResponse> GetAdminAnalyticsAsync()
        {
            var analytics = await _analyticsRepository.GetAdminAnalyticsAsync();
            var courses = await _courseService.GetAllCoursesPagedAsync(new CourseSearchQuery { PageNumber = 1, PageSize = 5, SortBy = "newest" });
            analytics.RecentCourses = courses.Courses.ToList();
            return analytics;
        }

        public async Task<InstructorAnalyticsResponse> GetInstructorAnalyticsAsync(int instructorId)
        {
            var analytics = await _analyticsRepository.GetInstructorAnalyticsAsync(instructorId);
            var courses = await _courseService.GetCoursesByInstructorPagedAsync(instructorId, new CourseSearchQuery { PageNumber = 1, PageSize = 4, SortBy = "newest" });
            analytics.RecentCourses = courses.Courses.ToList();
            return analytics;
        }

        public async Task<LearnerAnalyticsResponse> GetLearnerAnalyticsAsync(int learnerId)
        {
            var analytics = await _analyticsRepository.GetLearnerAnalyticsAsync(learnerId);
            var enrollments = await _enrollmentService.GetMyEnrollmentsPagedAsync(learnerId, 1, 4);
            analytics.MyCourses = enrollments.Enrollments.ToList();
            return analytics;
        }
    }
}
