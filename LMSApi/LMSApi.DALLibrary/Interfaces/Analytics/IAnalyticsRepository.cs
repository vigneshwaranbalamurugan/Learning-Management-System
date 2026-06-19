using LMSApi.ModelLibrary.DTOs;
using System.Threading.Tasks;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IAnalyticsRepository
    {
        Task<AdminAnalyticsResponse> GetAdminAnalyticsAsync();
        Task<InstructorAnalyticsResponse> GetInstructorAnalyticsAsync(int instructorId);
        Task<LearnerAnalyticsResponse> GetLearnerAnalyticsAsync(int learnerId);
    }
}
