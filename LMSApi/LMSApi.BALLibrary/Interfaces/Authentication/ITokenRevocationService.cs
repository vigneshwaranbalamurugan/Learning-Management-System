using System;
using System.Threading.Tasks;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ITokenRevocationService
    {
        Task BlockAccessTokenAsync(string jti, TimeSpan ttl);
        Task<bool> IsAccessTokenBlockedAsync(string jti);
    }
}
