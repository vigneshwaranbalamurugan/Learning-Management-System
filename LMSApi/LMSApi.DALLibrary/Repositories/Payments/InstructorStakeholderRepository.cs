using System.Threading.Tasks;
using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class InstructorStakeholderRepository : IInstructorStakeholderRepository
    {
        private readonly LMSDbContext _context;

        public InstructorStakeholderRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task<InstructorStakeholder?> GetByLinkedAccountIdAsync(int linkedAccountId)
        {
            return await _context.InstructorStakeholders
                .FirstOrDefaultAsync(s => s.InstructorLinkedAccountId == linkedAccountId);
        }

        public async Task AddAsync(InstructorStakeholder stakeholder)
        {
            _context.InstructorStakeholders.Add(stakeholder);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(InstructorStakeholder stakeholder)
        {
            _context.Entry(stakeholder).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
