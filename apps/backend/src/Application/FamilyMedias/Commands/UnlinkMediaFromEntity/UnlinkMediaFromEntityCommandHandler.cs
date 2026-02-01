using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.FamilyMedias.Commands.UnlinkMediaFromEntity;

public class UnlinkMediaFromEntityCommandHandler : IRequestHandler<UnlinkMediaFromEntityCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public UnlinkMediaFromEntityCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    public async Task<Result> Handle(UnlinkMediaFromEntityCommand request, CancellationToken cancellationToken)
    {
        var familyMedia = await _context.FamilyMedia
            .AsNoTracking()
            .FirstOrDefaultAsync(fm => fm.Id == request.FamilyMediaId && !fm.IsDeleted, cancellationToken);

        if (familyMedia == null)
        {
            return Result.Failure(string.Format(ErrorMessages.NotFound, $"FamilyMedia with ID {request.FamilyMediaId}"), ErrorSources.NotFound);
        }

        // Authorization check for the family associated with the media - only applies if media is associated with a family
        if (familyMedia.FamilyId.HasValue && !_authorizationService.CanManageFamily(familyMedia.FamilyId.Value))
        {
            return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var mediaLink = await _context.MediaLinks
            .FirstOrDefaultAsync(ml =>
                ml.FamilyMediaId == request.FamilyMediaId &&
                ml.RefType == request.RefType &&
                ml.RefId == request.RefId, cancellationToken);

        if (mediaLink == null)
        {
            return Result.Failure("Media link not found.", ErrorSources.NotFound);
        }

        _context.MediaLinks.Remove(mediaLink);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
