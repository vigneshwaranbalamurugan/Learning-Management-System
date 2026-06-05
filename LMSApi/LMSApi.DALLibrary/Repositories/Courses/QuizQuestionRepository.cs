using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.DALLibrary.Repositories;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class QuizQuestionRepository : AbstractRepository<int, QuizQuestions>, IQuizQuestionRepository
    {
        public QuizQuestionRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<QuizQuestions?> GetQuestionWithAnswersAsync(int id)
        {
            return await _context.QuizQuestions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);
        }
    }
}
