using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.DALLibrary.Repositories;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class QuizAnswerRepository : AbstractRepository<int, QuizAnswers>, IQuizAnswerRepository
    {
        public QuizAnswerRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task AddRangeAsync(IEnumerable<QuizAnswers> answers)
        {
            await _context.QuizAnswers.AddRangeAsync(answers);
            await _context.SaveChangesAsync();
        }
    }
}
