using System;
using System.Threading.Tasks;
using LMSApi.BALLibrary.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace LMSApi.BALLibrary.Services
{
    public class TokenRevocationService : ITokenRevocationService
    {
        private readonly IDistributedCache _cache;
        
        public TokenRevocationService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task BlockAccessTokenAsync(string jti, TimeSpan ttl)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };
            
            await _cache.SetStringAsync($"blocklist:{jti}", "1", options);
        }

        public async Task<bool> IsAccessTokenBlockedAsync(string jti)
        {
            var value = await _cache.GetStringAsync($"blocklist:{jti}");
            return value != null;
        }
    }
}
