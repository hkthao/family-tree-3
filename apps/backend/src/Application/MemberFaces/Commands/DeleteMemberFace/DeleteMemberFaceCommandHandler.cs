using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Knowledge;
using backend.Domain.Events.MemberFaces; // Add this line
using Microsoft.Extensions.Logging;
using backend.Application.MemberFaces.Messages; // Add this using statement

namespace backend.Application.MemberFaces.Commands.DeleteMemberFace;

public class DeleteMemberFaceCommandHandler : IRequestHandler<DeleteMemberFaceCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<DeleteMemberFaceCommandHandler> _logger;
    private readonly IMessageBus _messageBus;

    public DeleteMemberFaceCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ILogger<DeleteMemberFaceCommandHandler> logger, IMessageBus messageBus)
    {
        _context = context;
        _authorizationService = authorizationService;
        _logger = logger;
        _messageBus = messageBus;
    }

    public async Task<Result<Unit>> Handle(DeleteMemberFaceCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MemberFaces
            .Include(mf => mf.Member)
            .FirstOrDefaultAsync(mf => mf.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<Unit>.Failure($"MemberFace with ID {request.Id} not found.", ErrorSources.NotFound);
        }

        if (entity.Member == null || !_authorizationService.CanAccessFamily(entity.Member.FamilyId))
        {
            return Result<Unit>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        _context.MemberFaces.Remove(entity);
        entity.AddDomainEvent(new MemberFaceDeletedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        // Publish message to message bus
        if (entity.Member != null)
        {
            var message = new MemberFaceDeletedMessage
            {
                MemberFaceId = entity.Id,
                VectorDbId = entity.VectorDbId,
                MemberId = entity.MemberId,
                FamilyId = entity.Member.FamilyId
            };
            await _messageBus.PublishAsync(MessageBusConstants.Exchanges.MemberFace, MessageBusConstants.RoutingKeys.MemberFaceDeleted, message, cancellationToken);
            _logger.LogInformation("Published MemberFaceDeletedMessage for MemberFace {MemberFaceId}.", entity.Id);
        } else {
            _logger.LogWarning("Cannot publish MemberFaceDeletedMessage for MemberFace {MemberFaceId} because Member is null.", entity.Id);
        }

        _logger.LogInformation("Deleted MemberFace {MemberFaceId}.", entity.Id);

        return Result<Unit>.Success(Unit.Value);
    }
}
