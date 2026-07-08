using LMSApi.ModelLibrary.DTOs;
using System.Threading.Tasks;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IAnalyticsRepository
    {
        Task<AdminAnalyticsResponse> GetAdminAnalyticsAsync();
        Task<System.Collections.Generic.List<RecentActivityDto>> GetAdminRecentActivitiesAsync(int pageNumber, int pageSize);
        Task<InstructorAnalyticsResponse> GetInstructorAnalyticsAsync(int instructorId);
        Task<LearnerAnalyticsResponse> GetLearnerAnalyticsAsync(int learnerId);
    }
}
