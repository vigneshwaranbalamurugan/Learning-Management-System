using LMSApi.DALLibrary.Contexts;
using Microsoft.EntityFrameworkCore;
using LMSApi.DALLibrary.Interfaces;

namespace LMSApi.DALLibrary.Repositories
{
    public abstract class AbstractRepository<K,T> : IRepository<K,T> where T : class
    {
        protected readonly LMSDbContext _context;

        public AbstractRepository(LMSDbContext context)
        {
            _context = context;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public virtual async Task<T> GetByIdAsync(K id)
        {
            return await _context.Set<T>().FindAsync(id)?? throw new KeyNotFoundException($"{typeof(T).Name} with id '{id}' not found.");
        }

        public virtual async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(K id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task BeginTransactionAsync()
        {
            if (_context.Database.CurrentTransaction != null)
                return;
            
            await _context.Database.BeginTransactionAsync();
        }

        public virtual async Task CommitTransactionAsync()
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

        public virtual async Task RollbackTransactionAsync()
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