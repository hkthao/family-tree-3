using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using backend.Application.Common.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace backend.Infrastructure.Services.RateLimiting;

public class UserRateLimiterPolicy : IRateLimiterPolicy<string>
{
    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var userId =
            httpContext.User.Identity?.IsAuthenticated == true
                ? httpContext.User.Identity!.Name!
                : httpContext.Connection.Id;

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: userId,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 250,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    }

    public Func<OnRejectedContext, CancellationToken, ValueTask> OnRejected =>
        (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return ValueTask.CompletedTask;
        };
}
