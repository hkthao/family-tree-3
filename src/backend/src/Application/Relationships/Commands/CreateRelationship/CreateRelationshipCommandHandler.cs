using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Events.Relationships;

namespace backend.Application.Relationships.Commands.CreateRelationship;

public class CreateRelationshipCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMediator mediator, IUser user) : IRequestHandler<CreateRelationshipCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMediator _mediator = mediator;
    private readonly IUser _user = user;

    public async Task<Result<Guid>> Handle(CreateRelationshipCommand request, CancellationToken cancellationToken)
    {
        if (!_user.Id.HasValue)
        {
            return Result<Guid>.Failure("User is not authenticated.", "Authentication");
        }

        // Authorization check: Get family ID from source member
        var sourceMember = await _context.Members.FindAsync(request.SourceMemberId);
        if (sourceMember == null)
        {
            return Result<Guid>.Failure($"Source member with ID {request.SourceMemberId} not found.", "NotFound");
        }

        if (!_authorizationService.CanManageFamily(sourceMember.FamilyId))
        {
            return Result<Guid>.Failure("Access denied. Only family managers or admins can create relationships.", "Forbidden");
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
