using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IQuizQuestionService
    {
         // ─── Question CRUD ──────────────────────────────────────────────────
        Task<QuizQuestionResponse> AddQuestionAsync(CreateQuizQuestionRequest request);
        Task<QuizQuestionResponse> UpdateQuestionAsync(int id, UpdateQuizQuestionRequest request);
        Task DeleteQuestionAsync(int id);
        Task<BulkUploadResult> BulkUploadQuestionsAsync(int quizId, Stream excelStream);

    }
}