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
        // Authorization check: Get family ID from source member
        var sourceMember = await _context.Members.FindAsync(request.SourceMemberId);
        if (sourceMember == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Source member with ID {request.SourceMemberId}"), ErrorSources.NotFound);
        }

        if (!_authorizationService.CanManageFamily(sourceMember.FamilyId))
        {
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var entity = new Relationship
        {
            SourceMemberId = request.SourceMemberId,
            TargetMemberId = request.TargetMemberId,
            Type = request.Type,
            Order = request.Order
        };

        _context.Relationships.Add(entity);
        entity.AddDomainEvent(new RelationshipCreatedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
