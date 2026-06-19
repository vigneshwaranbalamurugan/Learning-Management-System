using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class PlatformFeeConfigRepository : IPlatformFeeConfigRepository
    {
        private readonly LMSDbContext _context;

        public PlatformFeeConfigRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task<PlatformFeeConfig?> GetActiveConfigAsync(FeeCategory category, DateTime at)
        {
            return await _context.PlatformFeeConfigs
                .Where(f => f.FeeCategory == category && f.EffectiveFrom <= at)
                .OrderByDescending(f => f.EffectiveFrom)
                .FirstOrDefaultAsync();
        }

        public async Task<PlatformFeeConfig?> GetCurrentAsync(FeeCategory category)
        {
            return await _context.PlatformFeeConfigs
                .Where(f => f.FeeCategory == category)
                .OrderByDescending(f => f.EffectiveFrom)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PlatformFeeConfig>> GetAllAsync(FeeCategory? category = null)
        {
            var query = _context.PlatformFeeConfigs
                .Include(f => f.CreatedByAdmin)
                .AsQueryable();

            if (category.HasValue)
                query = query.Where(f => f.FeeCategory == category.Value);

            return await query
                .OrderByDescending(f => f.EffectiveFrom)
                .ToListAsync();
        }

        public async Task AddAsync(PlatformFeeConfig config)
        {
            _context.PlatformFeeConfigs.Add(config);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PlatformFeeConfig config)
        {
            _context.PlatformFeeConfigs.Update(config);
            await _context.SaveChangesAsync();
        }
    }
}
