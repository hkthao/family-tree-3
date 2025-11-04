using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Relationships.Commands.UpdateRelationship;

public class UpdateRelationshipCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService) : IRequestHandler<UpdateRelationshipCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<bool>> Handle(UpdateRelationshipCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanManageFamily(request.FamilyId))
            return Result<bool>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);

        var family = await _context.Families
            .Include(f => f.Relationships)
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (family == null)
        {
            return Result<bool>.Failure(string.Format(ErrorMessages.NotFound, $"Family with ID {request.FamilyId}"), ErrorSources.NotFound);
        }

        var relationship = family.Relationships.FirstOrDefault(r => r.Id == request.Id);
        if (relationship == null)
        {
            return Result<bool>.Failure(string.Format(ErrorMessages.NotFound, $"Relationship with ID {request.Id}"), ErrorSources.NotFound);
        }

        relationship.Update(request.SourceMemberId, request.TargetMemberId, request.Type, request.Order);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
