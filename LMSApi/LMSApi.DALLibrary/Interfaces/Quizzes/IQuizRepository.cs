using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IQuizRepository : IRepository<int, Quzzes>
    {
        Task<IEnumerable<Quzzes>> GetQuizzesBySectionAsync(int sectionId);
        Task<Quzzes> GetQuizWithQuestionsAsync(int quizId);
    }
}
