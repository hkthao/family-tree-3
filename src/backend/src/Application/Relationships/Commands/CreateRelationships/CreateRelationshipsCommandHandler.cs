using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Services;
using backend.Domain.Entities;
using backend.Domain.Enums;
using MediatR;
using backend.Application.Relationships.Commands.CreateRelationship;

namespace backend.Application.Relationships.Commands.CreateRelationships;

public class CreateRelationshipsCommandHandler : IRequestHandler<CreateRelationshipsCommand, Result<List<Guid>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IAuthorizationService _authorizationService;
    private readonly FamilyAuthorizationService _familyAuthorizationService;
    private readonly IMediator _mediator;

    public CreateRelationshipsCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IAuthorizationService authorizationService,
        FamilyAuthorizationService familyAuthorizationService,
        IMediator mediator)
    {
        _context = context;
        _user = user;
        _authorizationService = authorizationService;
        _familyAuthorizationService = familyAuthorizationService;
        _mediator = mediator;
    }

    public async Task<Result<List<Guid>>> Handle(CreateRelationshipsCommand request, CancellationToken cancellationToken)
    {
        var createdRelationshipIds = new List<Guid>();

        foreach (var relationshipInput in request.Relationships)
        {
            var createRelationshipCommand = new CreateRelationshipCommand
            {
                SourceMemberId = relationshipInput.SourceMemberId,
                TargetMemberId = relationshipInput.TargetMemberId,
                Type = relationshipInput.Type,
                Order = relationshipInput.Order
            };

            var result = await _mediator.Send(createRelationshipCommand, cancellationToken);

            if (!result.IsSuccess)
            {
                return Result<List<Guid>>.Failure(result.Error ?? "Unknown error during single relationship creation.", result.ErrorSource ?? "Unknown");
            }

            createdRelationshipIds.Add(result.Value);
        }

        return Result<List<Guid>>.Success(createdRelationshipIds);
    }
}
