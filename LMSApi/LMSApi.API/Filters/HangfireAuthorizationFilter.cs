using Hangfire.Dashboard;

namespace LMSApi.API.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            
            // Allow all authenticated users to see the Dashboard (potentially restricted by role)
            // return httpContext.User.Identity?.IsAuthenticated == true && httpContext.User.IsInRole("Admin"); 
            
            // WARNING: Allowing all access to the Dashboard for Development purposes.
            // In Production, this must be secured (e.g., Basic Auth, or checking a cookie).
            return true;
        }
    }
}
