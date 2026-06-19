using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using System.Threading.Tasks;

namespace LMSApi.BALLibrary.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _analyticsRepository;

        public AnalyticsService(IAnalyticsRepository analyticsRepository)
        {
            _analyticsRepository = analyticsRepository;
        }

        public async Task<AdminAnalyticsResponse> GetAdminAnalyticsAsync()
        {
            return await _analyticsRepository.GetAdminAnalyticsAsync();
        }

        public async Task<InstructorAnalyticsResponse> GetInstructorAnalyticsAsync(int instructorId)
        {
            return await _analyticsRepository.GetInstructorAnalyticsAsync(instructorId);
        }

        public async Task<LearnerAnalyticsResponse> GetLearnerAnalyticsAsync(int learnerId)
        {
            return await _analyticsRepository.GetLearnerAnalyticsAsync(learnerId);
        }
    }
}
