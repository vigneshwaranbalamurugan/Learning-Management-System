using System.Threading.Tasks;
using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class InstructorPayoutProductRepository : IInstructorPayoutProductRepository
    {
        private readonly LMSDbContext _context;

        public InstructorPayoutProductRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task<InstructorPayoutProduct?> GetByLinkedAccountIdAsync(int linkedAccountId)
        {
            return await _context.InstructorPayoutProducts
                .FirstOrDefaultAsync(p => p.InstructorLinkedAccountId == linkedAccountId);
        }

        public async Task AddAsync(InstructorPayoutProduct product)
        {
            _context.InstructorPayoutProducts.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(InstructorPayoutProduct product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
