using backend.Application.Common.Interfaces.Core;
using Hangfire.Dashboard;

namespace backend.Web;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Allow all authenticated users to access the Dashboard
        // In a real application, you'd want more granular control,
        // e.g., checking for an admin role.
        var httpContext = context.GetHttpContext();
        if (httpContext == null) return false;

        var currentUserService = httpContext.RequestServices.GetRequiredService<ICurrentUser>();
        return currentUserService.IsAuthenticated;
    }
}
