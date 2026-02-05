using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.Users.Specifications;

namespace backend.Application.UserPreferences.Commands.SaveUserPreferences;

public class SaveUserPreferencesCommandHandler(IApplicationDbContext context, ICurrentUser user) : IRequestHandler<SaveUserPreferencesCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _user = user;

    public async Task<Result> Handle(SaveUserPreferencesCommand request, CancellationToken cancellationToken)
    {
        var userSpec = new UserByIdWithPreferenceSpec(_user.UserId);
        var user = await _context.Users
            .WithSpecification(userSpec)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            return Result.Failure($"User with ID {_user.UserId} not found.");
        }

        user.UpdatePreference(request.Theme, request.Language);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
