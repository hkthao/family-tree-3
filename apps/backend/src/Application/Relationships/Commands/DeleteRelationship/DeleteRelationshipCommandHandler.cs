using Ardalis.Specification.EntityFrameworkCore; // Added for WithSpecification
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;
using backend.Domain.Events.Relationships; // Added for FamilyByIdWithRelationshipsSpecification

namespace backend.Application.Relationships.Commands.DeleteRelationship;

public class DeleteRelationshipCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser currentUser, IDateTime dateTime) : IRequestHandler<DeleteRelationshipCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IDateTime _dateTime = dateTime;

    public async Task<Result<bool>> Handle(DeleteRelationshipCommand request, CancellationToken cancellationToken)
    {
        var relationship = await _context.Relationships.FindAsync(request.Id);
        if (relationship == null)
        {
            return Result<bool>.Failure(string.Format(ErrorMessages.NotFound, $"Relationship with ID {request.Id}"), ErrorSources.NotFound);
        }

        if (!_authorizationService.CanManageFamily(relationship.FamilyId))
        {
            return Result<bool>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var family = await _context.Families
            .WithSpecification(new FamilyByIdWithRelationshipsSpecification(relationship.FamilyId))
            .FirstOrDefaultAsync(cancellationToken);

        if (family == null)
        {
            return Result<bool>.Failure(string.Format(ErrorMessages.NotFound, $"Family with ID {relationship.FamilyId}"), ErrorSources.NotFound);
        }

        family.RemoveRelationship(request.Id);
        family.AddDomainEvent(new RelationshipDeletedEvent(relationship));

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
