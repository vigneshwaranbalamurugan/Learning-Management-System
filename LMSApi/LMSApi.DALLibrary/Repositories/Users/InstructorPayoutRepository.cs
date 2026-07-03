using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class InstructorPayoutRepository : IInstructorPayoutRepository
    {
        private readonly LMSDbContext _context;

        public InstructorPayoutRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(InstructorPayout payout)
        {
            _context.InstructorPayouts.Add(payout);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(InstructorPayout payout)
        {
            _context.Entry(payout).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<InstructorPayout?> GetByIdAsync(int id)
        {
            return await _context.InstructorPayouts
                .Include(p => p.Instructor)
                .Include(p => p.Payment).ThenInclude(pay => pay.Course)
                .Include(p => p.Payment).ThenInclude(pay => pay.User).ThenInclude(u => u.UserProfile)
                .Include(p => p.PayoutAccount)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<InstructorPayout?> GetByRazorpayPayoutIdAsync(string razorpayPayoutId)
        {
            return await _context.InstructorPayouts
                .Include(p => p.Instructor)
                .FirstOrDefaultAsync(p => p.RazorpayPayoutId == razorpayPayoutId);
        }

        public async Task<IEnumerable<InstructorPayout>> GetByInstructorAsync(int instructorId)
        {
            return await _context.InstructorPayouts
                .Where(p => p.InstructorId == instructorId)
                .Include(p => p.Payment).ThenInclude(pay => pay.Course)
                .Include(p => p.Payment).ThenInclude(pay => pay.User).ThenInclude(u => u.UserProfile)
                .Include(p => p.PayoutAccount)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<InstructorPayout>> GetAllAsync()
        {
            return await _context.InstructorPayouts
                .Include(p => p.Instructor).ThenInclude(i => i.UserProfile)
                .Include(p => p.Payment).ThenInclude(pay => pay.Course)
                .Include(p => p.Payment).ThenInclude(pay => pay.User).ThenInclude(u => u.UserProfile)
                .Include(p => p.PayoutAccount)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<InstructorPayout>> GetByStatusAsync(PayoutStatus status)
        {
            return await _context.InstructorPayouts
                .Where(p => p.Status == status)
                .Include(p => p.Instructor)
                .Include(p => p.Payment).ThenInclude(pay => pay.Course)
                .Include(p => p.Payment).ThenInclude(pay => pay.User).ThenInclude(u => u.UserProfile)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalEarningsAsync(int instructorId)
        {
            return await _context.InstructorPayouts
                .Where(p => p.InstructorId == instructorId && p.Status == PayoutStatus.Processed)
                .SumAsync(p => p.Amount);
        }
    }
}
