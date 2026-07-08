using LMSApi.ModelLibrary.DTOs;
using System.Threading.Tasks;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IAnalyticsService
    {
        Task<AdminAnalyticsResponse> GetAdminAnalyticsAsync();
        Task<System.Collections.Generic.List<RecentActivityDto>> GetAdminRecentActivitiesAsync(int pageNumber, int pageSize);
        Task<InstructorAnalyticsResponse> GetInstructorAnalyticsAsync(int instructorId);
        Task<LearnerAnalyticsResponse> GetLearnerAnalyticsAsync(int learnerId);
    }
}
