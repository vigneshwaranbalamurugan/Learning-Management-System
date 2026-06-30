using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace LMSApi.API.Filters
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class IdempotencyAttribute : Attribute, IAsyncActionFilter
    {
        private const string IdempotencyHeader = "Idempotency-Key";
        private const string ProcessingState = "__processing__";

        public bool Required { get; set; } = false;
        public int TtlMinutes { get; set; } = 24 * 60;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(IdempotencyHeader, out var headerValue))
            {
                if (Required)
                {
                    context.Result = new BadRequestObjectResult(new { Message = "Idempotency-Key header is required." });
                    return;
                }

                // No idempotency key provided, skip
                await next();
                return;
            }

            var idempotencyKey = headerValue.ToString();
            var multiplexer = context.HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
            var db = multiplexer.GetDatabase();

            // Generate a unique cache key for this user + path + idempotency key
            var userId = context.HttpContext.User.Identity?.IsAuthenticated == true ? context.HttpContext.User.Identity.Name : "anonymous";
            var requestPath = context.HttpContext.Request.Path.ToString();
            var cacheKey = $"lms:idempotency:{HashKey($"{userId}:{requestPath}:{idempotencyKey}")}";

            var expiry = TimeSpan.FromMinutes(TtlMinutes);

            // Attempt to acquire the lock using SET NX EX
            bool acquired = await db.StringSetAsync(cacheKey, ProcessingState, expiry, When.NotExists);

            if (!acquired)
            {
                // Key already exists. Check what's inside.
                var cachedValue = await db.StringGetAsync(cacheKey);
                if (cachedValue == ProcessingState)
                {
                    // Another request is currently processing this idempotency key
                    context.Result = new ConflictObjectResult(new { Message = "A request with this Idempotency-Key is currently being processed." });
                    return;
                }

                if (!cachedValue.IsNullOrEmpty)
                {
                    var cachedResult = JsonSerializer.Deserialize<CachedResult>(cachedValue.ToString(), _jsonOptions);
                    if (cachedResult != null)
                    {
                        context.Result = new ObjectResult(cachedResult.Value)
                        {
                            StatusCode = cachedResult.StatusCode
                        };
                        return;
                    }
                }
            }

            try
            {
                var executedContext = await next();

                if (executedContext.Exception == null && executedContext.Result is ObjectResult objectResult)
                {
                    var resultToCache = new CachedResult
                    {
                        StatusCode = objectResult.StatusCode ?? 200,
                        Value = objectResult.Value
                    };

                    await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(resultToCache, _jsonOptions), expiry);
                }
                else if (executedContext.Exception != null || (executedContext.Result is StatusCodeResult statusCodeResult && statusCodeResult.StatusCode >= 400))
                {
                    // If the action didn't return a successful result, clear the key to allow retries.
                    await db.KeyDeleteAsync(cacheKey);
                }
            }
            catch
            {
                // On any unhandled exception, clear the key to allow retries.
                await db.KeyDeleteAsync(cacheKey);
                throw;
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
