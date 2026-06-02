using System.Security.Claims;

namespace LMSApi.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? user.FindFirstValue("sub")
                        ?? user.FindFirstValue("nameid");

            // If the resolved claim is not a valid integer, try to find another standard claim that is
            if (value == null || !int.TryParse(value, out _))
            {
                var numericClaim = user.Claims.FirstOrDefault(c =>
                    (c.Type == ClaimTypes.NameIdentifier || c.Type == "sub" || c.Type == "nameid" || c.Type == "uid" || c.Type == "userid")
                    && int.TryParse(c.Value, out _));

                if (numericClaim != null)
                {
                    value = numericClaim.Value;
                }
            }

            if (value == null)
            {
                throw new UnauthorizedAccessException("User ID claim not found in token.");
            }

            return int.TryParse(value, out var id)
                ? id
                : throw new UnauthorizedAccessException("User ID claim is not a valid integer.");
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
