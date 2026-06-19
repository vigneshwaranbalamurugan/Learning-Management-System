using LMSApi.DALLibrary.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LMSApi.API.Services
{
    public class CurrentUserProvider : ICurrentUserProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetCurrentUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var claim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)
                            ?? httpContext.User.FindFirst("sub");
                if (claim != null && int.TryParse(claim.Value, out int userId))
                {
                    return userId;
                }
            }
            return null;
        }
    }
}
