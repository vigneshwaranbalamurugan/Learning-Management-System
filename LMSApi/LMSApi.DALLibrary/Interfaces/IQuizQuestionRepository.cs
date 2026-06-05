using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IQuizQuestionRepository : IRepository<int, QuizQuestions>
    {
        Task<QuizQuestions?> GetQuestionWithAnswersAsync(int id);
    }
}
