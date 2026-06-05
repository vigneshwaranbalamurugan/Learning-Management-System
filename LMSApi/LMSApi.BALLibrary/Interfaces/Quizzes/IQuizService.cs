using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IQuizService
    {
        // ─── Quiz CRUD ──────────────────────────────────────────────────────
        Task<IEnumerable<QuizResponse>> GetQuizzesBySectionAsync(int sectionId);
        Task<QuizDetailResponse> GetQuizByIdAsync(int id);
        Task<QuizResponse> CreateQuizAsync(CreateQuizRequest request);
        Task<QuizResponse> UpdateQuizAsync(int id, UpdateQuizRequest request);
        Task DeleteQuizAsync(int id);
        Task<QuizResponse> PublishQuizAsync(int quizId, PublishQuizRequest request);

        // ─── Question CRUD ──────────────────────────────────────────────────
        Task<QuizQuestionResponse> AddQuestionAsync(CreateQuizQuestionRequest request);
        Task<QuizQuestionResponse> UpdateQuestionAsync(int id, UpdateQuizQuestionRequest request);
        Task DeleteQuestionAsync(int id);

        // ─── Student Quiz-Taking ────────────────────────────────────────────
        Task<QuizStudentDetailResponse> GetQuizForStudentAsync(int quizId);
        Task<StartAttemptResponse> StartAttemptAsync(int quizId, int userId);
        Task<QuizAttemptResponse> SubmitQuizAsync(int userId, SubmitQuizRequest request);
        Task<IEnumerable<QuizAttemptResponse>> GetUserAttemptsAsync(int quizId, int userId);
        Task<QuizAttemptDetailResponse> GetAttemptDetailAsync(int attemptId);
        Task<GetRemainingAttemptsResponse> GetRemainingAttemptsAsync(int quizId, int userId);
    }
}
