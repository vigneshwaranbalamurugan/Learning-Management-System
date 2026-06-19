using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IQuizService
    {
        // ─── Quiz CRUD ──────────────────────────────────────────────────────
        Task<IEnumerable<QuizResponse>> GetQuizzesBySectionAsync(int sectionId, int? currentUserId = null, bool isAdmin = false);
        Task<QuizDetailResponse> GetQuizByIdAsync(int id, int? currentUserId = null, bool isAdmin = false);
        Task<QuizResponse> CreateQuizAsync(CreateQuizRequest request);
        Task<QuizResponse> UpdateQuizAsync(int id, UpdateQuizRequest request);
        Task DeleteQuizAsync(int id);
        Task<QuizResponse> PublishQuizAsync(int quizId, PublishQuizRequest request);
    }
}
