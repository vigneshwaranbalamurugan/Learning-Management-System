using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IQuizAttemptRepository : IRepository<int, QuizAttempts>
    {
        Task<IEnumerable<QuizAttempts>> GetAttemptsByQuizAndUserAsync(int quizId, int userId);
        Task<QuizAttempts> GetAttemptWithAnswersAsync(int attemptId);
        Task<int> GetAttemptCountAsync(int quizId, int userId);
        Task<double> CalculateScoreAsync(int attemptId);
        Task<bool> CalculatePassStatusAsync(int attemptId);
        Task<int> GetRemainingAttemptsAsync(int quizId, int userId);
        Task<int> GetPassedQuizzesCountAsync(int userId, List<int> quizIds);
        Task<QuizAttempts?> GetInProgressAttemptAsync(int quizId, int userId);
    }
}
