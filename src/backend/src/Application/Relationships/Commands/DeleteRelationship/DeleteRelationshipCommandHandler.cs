using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Events.Relationships;

namespace backend.Application.Relationships.Commands.DeleteRelationship;

public class DeleteRelationshipCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IUser currentUser, IDateTime dateTime) : IRequestHandler<DeleteRelationshipCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IUser _currentUser = currentUser;
    private readonly IDateTime _dateTime = dateTime;

    public async Task<Result<bool>> Handle(DeleteRelationshipCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Relationships.FindAsync(request.Id);
        if (entity == null)
            return Result<bool>.Failure(string.Format(ErrorMessages.NotFound, $"Relationship with ID {request.Id}"), ErrorSources.NotFound);

        // Authorization check: Get family ID from source member
        var sourceMember = await _context.Members.FindAsync(entity.SourceMemberId);
        if (sourceMember == null)
        {
            return Result<bool>.Failure(string.Format(ErrorMessages.NotFound, $"Source member for relationship {request.Id}"), ErrorSources.NotFound);
        }

        if (!_authorizationService.CanManageFamily(sourceMember.FamilyId))
        {
            return Result<bool>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        entity.IsDeleted = true;
        entity.DeletedBy = _currentUser.Id?.ToString();
        entity.DeletedDate = _dateTime.Now;

        entity.AddDomainEvent(new RelationshipDeletedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
