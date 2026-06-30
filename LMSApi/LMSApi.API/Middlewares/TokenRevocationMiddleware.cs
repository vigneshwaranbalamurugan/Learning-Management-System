using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using LMSApi.BALLibrary.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LMSApi.API.Middlewares
{
    public class TokenRevocationMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenRevocationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITokenRevocationService tokenRevocationService)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var jtiClaim = context.User.FindFirst(JwtRegisteredClaimNames.Jti);
                if (jtiClaim != null)
                {
                    var isBlocked = await tokenRevocationService.IsAccessTokenBlockedAsync(jtiClaim.Value);
                    if (isBlocked)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        
                        var errorResponse = new
                        {
                            success = false,
                            statusCode = StatusCodes.Status401Unauthorized,
                            message = "Token has been revoked.",
                            traceId = context.TraceIdentifier
                        };

                        var json = System.Text.Json.JsonSerializer.Serialize(errorResponse);
                        await context.Response.WriteAsync(json);
                        return; // short-circuit
                    }
                }
            }

            await _next(context);
        }
    }
}
