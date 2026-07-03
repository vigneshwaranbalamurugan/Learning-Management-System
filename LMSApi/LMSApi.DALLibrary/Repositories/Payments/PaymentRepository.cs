using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Enums;
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
                .Include(p => p.User).ThenInclude(u => u.UserProfile)
                .Include(p => p.Course).ThenInclude(c => c.Instructor).ThenInclude(i => i.UserProfile)
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

        public async Task<IEnumerable<Payments>> GetPaymentsByInstructorAsync(int instructorId)
        {
            return await _context.Payments
                .Where(p => p.Course.InstructorId == instructorId
                    && (p.Status == PaymentStatus.Completed || p.Status == PaymentStatus.Transferred))
                .Include(p => p.User).ThenInclude(u => u.UserProfile)
                .Include(p => p.Course)
                .Include(p => p.InstructorPayouts)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Payments> Items, int TotalCount)> GetPagedAsync(
            string? search, PaymentStatus? status, DateTime? dateFrom, DateTime? dateTo,
            int page, int pageSize)
        {
            var query = _context.Payments
                .Include(p => p.User).ThenInclude(u => u.UserProfile)
                .Include(p => p.Course).ThenInclude(c => c.Instructor).ThenInclude(i => i.UserProfile)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(p =>
                    p.Course.Title.ToLower().Contains(lower) ||
                    p.User.UserProfile.FirstName.ToLower().Contains(lower) ||
                    p.User.UserProfile.LastName.ToLower().Contains(lower) ||
                    (p.ProviderPaymentId != null && p.ProviderPaymentId.ToLower().Contains(lower)));
            }

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            if (dateFrom.HasValue)
                query = query.Where(p => p.CreatedAt >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(p => p.CreatedAt <= dateTo.Value.AddDays(1));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task BeginTransactionAsync()
        {
            if (_context.Database.CurrentTransaction != null)
                return;
            
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            var transaction = _context.Database.CurrentTransaction;
            if (transaction != null)
            {
                try
                {
                    await transaction.CommitAsync();
                }
                finally
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            var transaction = _context.Database.CurrentTransaction;
            if (transaction != null)
            {
                try
                {
                    await transaction.RollbackAsync();
                }
                finally
                {
                    await transaction.DisposeAsync();
                }
            }
        }
    }
}
