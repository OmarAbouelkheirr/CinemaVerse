using Hangfire.Dashboard;
using System.Security.Claims;

namespace CinemaVerse.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Allow all authenticated users to see the Dashboard (with "Admin" role)
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                return httpContext.User.IsInRole("Admin");
            }

            return false;
        }
    }
}
