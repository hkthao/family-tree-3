using backend.Application.Common.Constants;
using backend.Application.Common.Models;
using backend.Application.Relationships.Commands.CreateRelationship;

namespace backend.Application.Relationships.Commands.CreateRelationships;

public class CreateRelationshipsCommandHandler(IMediator mediator) : IRequestHandler<CreateRelationshipsCommand, Result<List<Guid>>>
{
    private readonly IMediator _mediator = mediator;

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
                return Result<List<Guid>>.Failure(result.Error ?? string.Format(ErrorMessages.UnexpectedError, "single relationship creation"), result.ErrorSource ?? ErrorSources.Exception);
            }

            createdRelationshipIds.Add(result.Value);
        }

        return Result<List<Guid>>.Success(createdRelationshipIds);
    }
}
