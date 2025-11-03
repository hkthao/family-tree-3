using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.UserPreferences.Commands.SaveUserPreferences;

public class SaveUserPreferencesCommandHandler(IApplicationDbContext context, ICurrentUser user) : IRequestHandler<SaveUserPreferencesCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser  _user = user;

    public async Task<Result> Handle(SaveUserPreferencesCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Preference)
            .FirstOrDefaultAsync(u => u.Id == _user.UserId, cancellationToken);

        if (user == null)
        {
            return Result.Failure($"User with ID {_user.UserId} not found.");
        }

        user.UpdatePreference(request.Theme.ToString(), request.Language.ToString());

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
