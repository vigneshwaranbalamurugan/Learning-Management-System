using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IQuizAnswerRepository : IRepository<int, QuizAnswers>
    {
        Task AddRangeAsync(IEnumerable<QuizAnswers> answers);
    }
}
