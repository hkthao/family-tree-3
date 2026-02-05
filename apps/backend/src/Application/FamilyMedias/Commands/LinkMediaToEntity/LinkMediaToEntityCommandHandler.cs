using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.FamilyMedias.Commands.LinkMediaToEntity;

public class LinkMediaToEntityCommandHandler : IRequestHandler<LinkMediaToEntityCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public LinkMediaToEntityCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    public async Task<Result<Guid>> Handle(LinkMediaToEntityCommand request, CancellationToken cancellationToken)
    {
        var familyMedia = await _context.FamilyMedia
            .AsNoTracking()
            .FirstOrDefaultAsync(fm => fm.Id == request.FamilyMediaId && !fm.IsDeleted, cancellationToken);

        if (familyMedia == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"FamilyMedia with ID {request.FamilyMediaId}"), ErrorSources.NotFound);
        }

        // Authorization check for the family associated with the media - only applies if media is associated with a family
        if (familyMedia.FamilyId.HasValue && !_authorizationService.CanManageFamily(familyMedia.FamilyId.Value))
        {
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // Check if link already exists
        var existingLink = await _context.MediaLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(ml =>
                ml.FamilyMediaId == request.FamilyMediaId &&
                ml.RefType == request.RefType &&
                ml.RefId == request.RefId, cancellationToken);

        if (existingLink != null)
        {
            return Result<Guid>.Failure("Media is already linked to this entity.", ErrorSources.Conflict);
        }

        // Create new link
        var mediaLink = new MediaLink
        {
            FamilyMediaId = request.FamilyMediaId,
            RefType = request.RefType,
            RefId = request.RefId
        };

        _context.MediaLinks.Add(mediaLink);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(mediaLink.Id);
    }
}
