using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.DALLibrary.Repositories;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class QuizAttemptRepository : AbstractRepository<int, QuizAttempts>, IQuizAttemptRepository
    {
        public QuizAttemptRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<QuizAttempts>> GetAttemptsByQuizAndUserAsync(int quizId, int userId)
        {
            return await _context.QuizAttempts
                .Where(a => a.QuizId == quizId && a.UserId == userId)
                .OrderByDescending(a => a.StartedAt)
                .ToListAsync();
        }

        public async Task<QuizAttempts> GetAttemptWithAnswersAsync(int attemptId)
        {
            return await _context.QuizAttempts
                .Include(a => a.Answers)
                    .ThenInclude(ans => ans.Question)
                .Include(a => a.Answers)
                    .ThenInclude(ans => ans.SelectedOption)
                .FirstOrDefaultAsync(a => a.Id == attemptId)
                ?? throw new KeyNotFoundException($"Quiz attempt with id '{attemptId}' not found.");
        }

        public async Task<int> GetAttemptCountAsync(int quizId, int userId)
        {
            return await _context.QuizAttempts
                .CountAsync(a => a.QuizId == quizId && a.UserId == userId);
        }

        public async Task<double> CalculateScoreAsync(int attemptId)
        {
            return await _context.Database
                .SqlQuery<double>($"SELECT calculate_quiz_score({attemptId})")
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CalculatePassStatusAsync(int attemptId)
        {
            return await _context.Database
                .SqlQuery<bool>($"SELECT calculate_pass_status({attemptId})")
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetRemainingAttemptsAsync(int quizId, int userId)
        {
            return await _context.Database
                .SqlQuery<int>($"SELECT get_remaining_attempts({quizId}, {userId})")
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetPassedQuizzesCountAsync(int userId, List<int> quizIds)
        {
            if (quizIds == null || !quizIds.Any()) return 0;

            return await _context.QuizAttempts
                .Where(a => quizIds.Contains(a.QuizId) && a.UserId == userId && a.IsPassed)
                .Select(a => a.QuizId)
                .Distinct()
                .CountAsync();
        }

        public async Task<QuizAttempts?> GetInProgressAttemptAsync(int quizId, int userId)
        {
            return await _context.QuizAttempts
                .Include(a => a.Answers)
                .FirstOrDefaultAsync(a => a.QuizId == quizId && a.UserId == userId && a.Status == AttemptStatus.InProgress);
        }
    }
}
