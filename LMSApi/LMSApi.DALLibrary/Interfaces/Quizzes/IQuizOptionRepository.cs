using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IQuizOptionRepository : IRepository<int, QuizOptions>
    {
        Task DeleteRangeAsync(IEnumerable<QuizOptions> options);
    }
}
