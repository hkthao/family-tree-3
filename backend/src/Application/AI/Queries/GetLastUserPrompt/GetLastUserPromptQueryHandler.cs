using MediatR;
using backend.Application.Common.Models;
using backend.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using backend.Application.UserProfiles.Specifications;
using Ardalis.Specification.EntityFrameworkCore;

namespace backend.Application.AI.Queries.GetLastUserPrompt;

/// <summary>
/// Handler for retrieving the last user prompt for a specific member.
/// </summary>
public class GetLastUserPromptQueryHandler : IRequestHandler<GetLastUserPromptQuery, Result<string?>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IAuthorizationService _authorizationService;

    public GetLastUserPromptQueryHandler(IApplicationDbContext context, IUser user, IAuthorizationService authorizationService)
    {
        _context = context;
        _user = user;
        _authorizationService = authorizationService;
    }

    public async Task<Result<string?>> Handle(GetLastUserPromptQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _user.Id;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Result<string?>.Failure("User is not authenticated.", "Authentication");
        }

        var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
        if (currentUserProfile == null)
        {
            return Result<string?>.Failure("User profile not found.", "NotFound");
        }

        // Authorization check: User must be a manager of the family the member belongs to, or an admin.
        var member = await _context.Members.FindAsync(request.MemberId, cancellationToken);
        if (member == null)
        {
            return Result<string?>.Failure($"Member with ID {request.MemberId} not found.", "NotFound");
        }

        if (!_authorizationService.IsAdmin() && !_authorizationService.CanManageFamily(member.FamilyId, currentUserProfile))
        {
            return Result<string?>.Failure("Access denied. Only family managers or admins can view last prompt.", "Forbidden");
        }

        var lastPrompt = await _context.AIBiographies
            .Where(b => b.MemberId == request.MemberId && b.UserPrompt != null)
            .OrderByDescending(b => b.Created)
            .Select(b => b.UserPrompt)
            .FirstOrDefaultAsync(cancellationToken);

        return Result<string?>.Success(lastPrompt);
    }
}
