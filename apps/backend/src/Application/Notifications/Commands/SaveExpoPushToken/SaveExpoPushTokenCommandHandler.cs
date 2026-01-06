using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Application.Notifications.Commands.SaveExpoPushToken;

public class SaveExpoPushTokenCommandHandler(IApplicationDbContext context, ICurrentUser currentUser, INotificationService notificationService) : IRequestHandler<SaveExpoPushTokenCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly INotificationService _notificationService = notificationService;

    public async Task<Result> Handle(SaveExpoPushTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId))
        {
            return Result.Failure("User ID cannot be empty.", "Validation");
        }

        // Retrieve all active ExpoPushTokens for the given UserId
        var expoPushTokens = await _context.UserPushTokens
            .Where(t => t.UserId.ToString() == request.UserId && t.IsActive)
            .Select(t => t.ExpoPushToken)
            .ToListAsync(cancellationToken);

        if (!expoPushTokens.Any())
        {
            // If no active tokens found, still call the service with an empty list
            // or consider it a success if nothing needs to be saved.
            // For now, let's call the service with an empty list to ensure sync.
            return Result.Success();
        }

        // Save the list of Expo Push Tokens with the Notification Service
        var saveResult = await _notificationService.SaveExpoPushTokenAsync(request.UserId, expoPushTokens.Cast<string?>().ToList(), cancellationToken);

        if (!saveResult.IsSuccess)
        {
            return saveResult;
        }

        return Result.Success();
    }
}
