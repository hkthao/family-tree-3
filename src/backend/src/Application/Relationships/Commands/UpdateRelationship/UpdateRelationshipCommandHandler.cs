using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Events.Relationships;

namespace backend.Application.Relationships.Commands.UpdateRelationship;

public class UpdateRelationshipCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService) : IRequestHandler<UpdateRelationshipCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<bool>> Handle(UpdateRelationshipCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Relationships.FindAsync(request.Id);
        if (entity == null)
            return Result<bool>.Failure(string.Format(ErrorMessages.NotFound, $"Relationship with ID {request.Id}"), ErrorSources.NotFound);

        // Authorization check: Get family ID from source member
        var sourceMember = await _context.Members.FindAsync(entity.SourceMemberId);
        if (sourceMember == null)
            return Result<bool>.Failure(string.Format(ErrorMessages.NotFound, $"Source member for relationship {request.Id}"), ErrorSources.NotFound);

        if (!_authorizationService.CanManageFamily(sourceMember.FamilyId))
            return Result<bool>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);

        entity.SourceMemberId = request.SourceMemberId;
        entity.TargetMemberId = request.TargetMemberId;
        entity.Type = request.Type;
        entity.Order = request.Order;

        entity.AddDomainEvent(new RelationshipUpdatedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
