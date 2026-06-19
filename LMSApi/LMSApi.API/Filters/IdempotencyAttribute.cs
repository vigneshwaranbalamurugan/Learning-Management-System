using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LMSApi.API.Filters
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class IdempotencyAttribute : Attribute, IAsyncActionFilter
    {
        private const string IdempotencyHeader = "Idempotency-Key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(IdempotencyHeader, out var headerValue))
            {
                // No idempotency key provided, skip
                await next();
                return;
            }

            var idempotencyKey = headerValue.ToString();
            var cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();

            // Generate a unique cache key for this user + path + idempotency key
            var userId = context.HttpContext.User.Identity?.IsAuthenticated == true ? context.HttpContext.User.Identity.Name : "anonymous";
            var requestPath = context.HttpContext.Request.Path.ToString();
            var cacheKey = $"Idempotency_{HashKey($"{userId}:{requestPath}:{idempotencyKey}")}";

            var cachedResponse = await cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedResponse))
            {
                var cachedResult = JsonSerializer.Deserialize<CachedResult>(cachedResponse);
                if (cachedResult != null)
                {
                    var result = new ObjectResult(cachedResult.Value)
                    {
                        StatusCode = cachedResult.StatusCode
                    };
                    context.Result = result;
                    return;
                }
            }

            var executedContext = await next();

            if (executedContext.Exception == null && executedContext.Result is ObjectResult objectResult)
            {
                var resultToCache = new CachedResult
                {
                    StatusCode = objectResult.StatusCode ?? 200,
                    Value = objectResult.Value
                };

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                };

                await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(resultToCache), cacheOptions);
            }
        }

        private static string HashKey(string input)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hashBytes);
        }

        private class CachedResult
        {
            public int StatusCode { get; set; }
            public object? Value { get; set; }
        }
    }
}
