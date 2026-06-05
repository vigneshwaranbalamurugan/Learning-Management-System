using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.DALLibrary.Repositories;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class QuizRepository : AbstractRepository<int, Quzzes>, IQuizRepository
    {
        public QuizRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Quzzes>> GetQuizzesBySectionAsync(int sectionId)
        {
            return await _context.Quizzes
                .Where(q => q.CourseSectionId == sectionId)
                .Include(q => q.Questions)
                .OrderBy(q => q.Order)
                .ToListAsync();
        }

        public async Task<Quzzes> GetQuizWithQuestionsAsync(int quizId)
        {
            return await _context.Quizzes
                .Include(q => q.Questions.OrderBy(qq => qq.SortOrder))
                    .ThenInclude(qq => qq.Answers)
                .FirstOrDefaultAsync(q => q.Id == quizId)
                ?? throw new KeyNotFoundException($"Quiz with id '{quizId}' not found.");
        }
    }
}
