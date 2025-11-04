using System.Security.Claims;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Web.Infrastructure;

public class NovuSubscriberCreationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NovuSubscriberCreationMiddleware> _logger;

    public NovuSubscriberCreationMiddleware(RequestDelegate next, ILogger<NovuSubscriberCreationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IApplicationDbContext dbContext, INotificationProviderFactory notificationProviderFactory, IServiceProvider serviceProvider)
    {
        // 1. Check if user is authenticated and user object is present
        // 2. Check if we haven't already processed this for the current request
        if (context.User.Identity?.IsAuthenticated == true &&
            !context.Items.ContainsKey("NovuSubscriberProcessed"))
        {
            // Mark as processed for this request to avoid re-entry
            context.Items["NovuSubscriberProcessed"] = true;

            var externalId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.AuthProviderId == externalId);

            // 3. Check if SubscriberId is missing
            if (user != null && string.IsNullOrEmpty(user.SubscriberId))
            {
                try
                {
                    _logger.LogInformation("User {UserId} does not have a Novu SubscriberId. Attempting to create one.", user.Id);

                    var novuProvider = notificationProviderFactory.GetProvider("Novu");
                    var subscriberId = $"user_{user.Id}"; // Or some other unique identifier

                    await novuProvider.SyncSubscriberAsync(
                        subscriberId,
                        user.Profile?.FirstName,
                        user.Profile?.LastName,
                        user.Email,
                        user.Profile?.Phone
                    );

                    // Re-fetch the user entity to ensure it's tracked by the current DbContext
                    var trackedUser = await dbContext.Users.FindAsync(user.Id);
                    if (trackedUser != null)
                    {
                        trackedUser.SubscriberId = subscriberId;
                        await dbContext.SaveChangesAsync(context.RequestAborted);
                        _logger.LogInformation("Successfully created Novu subscriber for user {UserId}", user.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Could not find user {UserId} in DbContext to update SubscriberId.", user.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating Novu subscriber for user {UserId}", user.Id);
                }
            }
        }

        await _next(context);
    }
}