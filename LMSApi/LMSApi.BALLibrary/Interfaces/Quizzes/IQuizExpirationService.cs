using System.Threading.Tasks;

namespace LMSApi.BALLibrary.Interfaces.Quizzes
{
    public interface IQuizExpirationService
    {
        Task ProcessExpiredQuizzesAsync();
    }
}
