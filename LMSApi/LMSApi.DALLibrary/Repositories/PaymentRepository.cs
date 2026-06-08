using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly LMSDbContext _context;

        public PaymentRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Payments item)
        {
            _context.Payments.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int key)
        {
            var payment = await GetByIdAsync(key);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Payments> GetByIdAsync(int key)
        {
            return await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Course)
                .Include(p => p.Enrollment)
                .FirstOrDefaultAsync(p => p.Id == key);
        }

        public async Task<IEnumerable<Payments>> GetAllAsync()
        {
            return await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Course)
                .ToListAsync();
        }

        public async Task UpdateAsync(Payments item)
        {
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<Payments?> GetByProviderOrderIdAsync(string providerOrderId)
        {
            return await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Course)
                .FirstOrDefaultAsync(p => p.ProviderOrderId == providerOrderId);
        }

        public async Task<IEnumerable<Payments>> GetPaymentsByUserAsync(int userId)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId)
                .Include(p => p.Course)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }
    }
}
