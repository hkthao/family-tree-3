using MediatR;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.AI.Commands.SaveAIBiography;

public class SaveAIBiographyCommandHandler : IRequestHandler<SaveAIBiographyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUser _user;

    public SaveAIBiographyCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IUser user)
    {
        _context = context;
        _authorizationService = authorizationService;
        _user = user;
    }

    public async Task<Result<Guid>> Handle(SaveAIBiographyCommand request, CancellationToken cancellationToken)
    {
        var member = await _context.Members.FindAsync(request.MemberId, cancellationToken);
        if (member == null)
        {
            return Result<Guid>.Failure($"Member with ID {request.MemberId} not found.", "NotFound");
        }

        var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
        if (currentUserProfile == null)
        {
            return Result<Guid>.Failure("User profile not found.", "NotFound");
        }

        // Authorization check: User must be a manager of the family the member belongs to, or an admin.
        if (!_authorizationService.IsAdmin() && !_authorizationService.CanManageFamily(member.FamilyId, currentUserProfile))
        {
            return Result<Guid>.Failure("Access denied. Only family managers or admins can save AI biographies.", "Forbidden");
        }

        var biography = await _context.AIBiographies
            .FirstOrDefaultAsync(b => b.MemberId == request.MemberId && b.Style == request.Style, cancellationToken);

        if (biography == null)
        {
            biography = new AIBiography
            {
                MemberId = request.MemberId,
                Style = request.Style,
                Content = request.Content,
                Provider = request.Provider,
                UserPrompt = request.UserPrompt,
                GeneratedFromDB = request.GeneratedFromDB,
                TokensUsed = request.TokensUsed,
                Metadata = null // TODO: Add relevant metadata if needed
            };
            _context.AIBiographies.Add(biography);
        }
        else
        {
            biography.Content = request.Content;
            biography.Provider = request.Provider;
            biography.UserPrompt = request.UserPrompt;
            biography.GeneratedFromDB = request.GeneratedFromDB;
            biography.TokensUsed = request.TokensUsed;
            biography.LastModified = DateTime.UtcNow; // Update timestamp
            biography.LastModifiedBy = _user.Id; // Update user
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(biography.Id);
    }
}
