using System.Security.Claims;

namespace LMSApi.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) 
                              ?? user.FindFirst("sub") 
                              ?? throw new UnauthorizedAccessException("Authenticated user ID was not found.");

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Authenticated user ID is not a valid integer.");
            }
            return userId;
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Email)
                   ?? user.FindFirstValue("email")
                   ?? throw new UnauthorizedAccessException("Authenticated user email was not found.");
        }

        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.IsInRole("Admin");
        }
    }
}
