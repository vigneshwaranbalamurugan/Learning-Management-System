using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IQuizAttemptService
    {
        
        // ─── Student Quiz-Taking ────────────────────────────────────────────
        Task<QuizStudentDetailResponse> GetQuizForStudentAsync(int quizId, int userId);
        Task<StartAttemptResponse> StartAttemptAsync(int quizId, int userId);
        Task<QuizAttemptResponse> SubmitQuizAsync(int quizId, int userId, SubmitQuizRequest request);
        Task<IEnumerable<QuizAttemptResponse>> GetUserAttemptsAsync(int quizId, int userId);
        Task<QuizAttemptDetailResponse> GetAttemptDetailAsync(int attemptId);
        Task<GetRemainingAttemptsResponse> GetRemainingAttemptsAsync(int quizId, int userId);

    }
}