using System;
using System.Threading.Tasks;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);
        Task InvalidateAsync(params string[] keys);
        Task RemoveAsync(string key);
    }
}
