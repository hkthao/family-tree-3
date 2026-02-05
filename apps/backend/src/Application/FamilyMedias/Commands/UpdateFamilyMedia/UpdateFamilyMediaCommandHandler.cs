using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;

namespace backend.Application.FamilyMedias.Commands.UpdateFamilyMedia;

public class UpdateFamilyMediaCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService) : IRequestHandler<UpdateFamilyMediaCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result> Handle(UpdateFamilyMediaCommand request, CancellationToken cancellationToken)
    {
        // Authorization check
        if (!_authorizationService.CanManageFamily(request.FamilyId))
        {
            return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var familyMedia = await _context.FamilyMedia.FirstOrDefaultAsync(fm => fm.Id == request.Id && !fm.IsDeleted, cancellationToken);

        if (familyMedia == null)
        {
            return Result.Failure(string.Format(ErrorMessages.NotFound, $"FamilyMedia with ID {request.Id}"), ErrorSources.NotFound);
        }

        familyMedia.FileName = request.FileName;
        familyMedia.MediaType = request.MediaType;
        familyMedia.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
