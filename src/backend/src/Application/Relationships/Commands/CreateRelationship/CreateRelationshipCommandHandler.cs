using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Events.Relationships;

namespace backend.Application.Relationships.Commands.CreateRelationship;

public class CreateRelationshipCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService) : IRequestHandler<CreateRelationshipCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<Guid>> Handle(CreateRelationshipCommand request, CancellationToken cancellationToken)
    {
        var sourceMember = await _context.Members.FindAsync(request.SourceMemberId, cancellationToken);

        if (sourceMember == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Source member with ID {request.SourceMemberId}"), ErrorSources.NotFound);
        }

        // Authorization check: Get family ID from source member
        if (!_authorizationService.CanManageFamily(sourceMember.FamilyId))
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);

        var family = await _context.Families
            .Include(f => f.Members)
            .FirstOrDefaultAsync(f => f.Id == sourceMember.FamilyId, cancellationToken);

        if (family == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Family with ID {sourceMember.FamilyId}"), ErrorSources.NotFound);
        }

        var relationship = family.AddRelationship(request.SourceMemberId, request.TargetMemberId, request.Type);
        relationship.Order = request.Order;
        _context.Relationships.Add(relationship);

        relationship.AddDomainEvent(new RelationshipCreatedEvent(relationship));

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(relationship.Id);
    }
}
