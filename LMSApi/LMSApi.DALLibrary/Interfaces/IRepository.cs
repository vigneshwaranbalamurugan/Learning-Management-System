namespace LMSApi.DALLibrary.Interfaces
{
    public interface IRepository<K,T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(K id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(K id);
    }
}