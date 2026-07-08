using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LMSApi.BALLibrary.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Utils
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;

        // Per-key semaphores to prevent cache stampede (Bug #3)
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var cachedString = await _cache.GetStringAsync(key);
                if (string.IsNullOrEmpty(cachedString))
                    return default;
                return JsonSerializer.Deserialize<T>(cachedString);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get value from cache for key {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions();
                if (expiry.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiry.Value;
                }
                var json = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, json, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set value to cache for key {Key}", key);
            }
        }

        // Bug #1 fixed: return type is now Task<T?> to honestly reflect nullability.
        // Bug #3 fixed: per-key SemaphoreSlim prevents cache stampede on concurrent misses.
        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            // Fast path: check cache first without locking
            try
            {
                var cachedString = await _cache.GetStringAsync(key);
                if (!string.IsNullOrEmpty(cachedString))
                {
                    var deserialized = JsonSerializer.Deserialize<T>(cachedString);
                    if (deserialized is not null)
                        return deserialized;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get value from cache for key {Key}", key);
            }

            // Slow path: acquire a per-key lock to prevent stampede
            var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            try
            {
                // Double-check inside the lock (another request may have populated the cache)
                try
                {
                    var cachedString = await _cache.GetStringAsync(key);
                    if (!string.IsNullOrEmpty(cachedString))
                    {
                        var deserialized = JsonSerializer.Deserialize<T>(cachedString);
                        if (deserialized is not null)
                            return deserialized;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get value from cache for key {Key} (inside lock)", key);
                }

                var value = await factory();

                if (value is not null)
                {
                    await SetAsync(key, value, expiry);
                }

                return value;
            }
            finally
            {
                semaphore.Release();
                // Clean up the semaphore if it's no longer needed to avoid unbounded growth
                _locks.TryRemove(key, out _);
            }
        }

        public async Task InvalidateAsync(params string[] keys)
        {
            foreach (var key in keys)
            {
                await RemoveAsync(key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove key from cache: {Key}", key);
            }
        }
    }
}

