using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UserPushTokens.Commands.RemoveUserPushToken;

public class RemoveUserPushTokenCommandHandler : IRequestHandler<RemoveUserPushTokenCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveUserPushTokenCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RemoveUserPushTokenCommand request, CancellationToken cancellationToken)
    {
        var userPushToken = await _context.UserPushTokens
            .FirstOrDefaultAsync(
                t => t.DeviceId == request.DeviceId &&
                     t.ExpoPushToken == request.ExpoPushToken &&
                     t.UserId == request.UserId,
                cancellationToken);

        if (userPushToken == null)
        {
            return Result.Failure("User push token not found or does not belong to the user.");
        }

        _context.UserPushTokens.Remove(userPushToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
