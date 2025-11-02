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

        var entity = new Relationship
        {
            SourceMemberId = request.SourceMemberId,
            TargetMemberId = request.TargetMemberId,
            Type = request.Type,
            Order = request.Order,
            FamilyId = sourceMember.FamilyId // Set FamilyId from source member
        };

        _context.Relationships.Add(entity);
        entity.AddDomainEvent(new RelationshipCreatedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
