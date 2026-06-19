using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class InstructorPayoutAccountRepository : IInstructorPayoutAccountRepository
    {
        private readonly LMSDbContext _context;

        public InstructorPayoutAccountRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task<InstructorPayoutAccount?> GetActiveByInstructorIdAsync(int instructorId)
        {
            return await _context.InstructorPayoutAccounts
                .Where(a => a.InstructorId == instructorId && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<InstructorPayoutAccount>> GetAllByInstructorIdAsync(int instructorId)
        {
            return await _context.InstructorPayoutAccounts
                .Where(a => a.InstructorId == instructorId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<InstructorPayoutAccount?> GetByIdAsync(int id)
        {
            return await _context.InstructorPayoutAccounts
                .Include(a => a.Instructor)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(InstructorPayoutAccount account)
        {
            _context.InstructorPayoutAccounts.Add(account);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(InstructorPayoutAccount account)
        {
            _context.Entry(account).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_context.Database.CurrentTransaction == null)
                await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_context.Database.CurrentTransaction != null)
                await _context.Database.CurrentTransaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            if (_context.Database.CurrentTransaction != null)
                await _context.Database.CurrentTransaction.RollbackAsync();
        }
    }
}
