using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories.AI
{
    public class LessonAiSummaryRepository : ILessonAiSummaryRepository
    {
        private readonly LMSDbContext _context;

        public LessonAiSummaryRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task<LessonAiSummary?> GetByLessonIdAsync(int lessonId)
        {
            return await _context.LessonAiSummaries
                .FirstOrDefaultAsync(s => s.LessonId == lessonId);
        }

        public async Task UpsertAsync(LessonAiSummary summary)
        {
            var existing = await _context.LessonAiSummaries
                .FirstOrDefaultAsync(s => s.LessonId == summary.LessonId);

            if (existing == null)
            {
                await _context.LessonAiSummaries.AddAsync(summary);
            }
            else
            {
                existing.Summary = summary.Summary;
                existing.KeyPointsJson = summary.KeyPointsJson;
                existing.Notes = summary.Notes;
                existing.Status = summary.Status;
                existing.GeneratedAt = summary.GeneratedAt;
                _context.LessonAiSummaries.Update(existing);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteByLessonIdAsync(int lessonId)
        {
            var existing = await _context.LessonAiSummaries
                .FirstOrDefaultAsync(s => s.LessonId == lessonId);
            if (existing != null)
            {
                _context.LessonAiSummaries.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }
    }
}
