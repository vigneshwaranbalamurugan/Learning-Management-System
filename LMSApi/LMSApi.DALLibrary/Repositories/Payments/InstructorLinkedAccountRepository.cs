using System.Linq;
using System.Threading.Tasks;
using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class InstructorLinkedAccountRepository : IInstructorLinkedAccountRepository
    {
        private readonly LMSDbContext _context;

        public InstructorLinkedAccountRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task<InstructorLinkedAccount?> GetActiveByInstructorIdAsync(int instructorId)
        {
            return await _context.InstructorLinkedAccounts
                .Include(a => a.Stakeholder)
                .Include(a => a.PayoutProduct)
                .Where(a => a.InstructorId == instructorId && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<InstructorLinkedAccount?> GetByIdAsync(int id)
        {
            return await _context.InstructorLinkedAccounts
                .Include(a => a.Instructor)
                .Include(a => a.Stakeholder)
                .Include(a => a.PayoutProduct)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<InstructorLinkedAccount?> GetByRazorpayAccountIdAsync(string razorpayAccountId)
        {
            return await _context.InstructorLinkedAccounts
                .Include(a => a.Stakeholder)
                .Include(a => a.PayoutProduct)
                .FirstOrDefaultAsync(a => a.RazorpayAccountId == razorpayAccountId);
        }

        public async Task AddAsync(InstructorLinkedAccount account)
        {
            _context.InstructorLinkedAccounts.Add(account);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(InstructorLinkedAccount account)
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
