using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.DALLibrary.Repositories;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class QuizOptionRepository : AbstractRepository<int, QuizOptions>, IQuizOptionRepository
    {
        public QuizOptionRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task DeleteRangeAsync(IEnumerable<QuizOptions> options)
        {
            _context.QuizOptions.RemoveRange(options);
            await _context.SaveChangesAsync();
        }
    }
}
