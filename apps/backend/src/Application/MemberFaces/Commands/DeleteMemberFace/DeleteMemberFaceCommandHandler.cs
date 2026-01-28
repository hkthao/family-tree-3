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
    private readonly IFaceApiService _faceApiService;
    private readonly IMessageBus _messageBus; // Add IMessageBus

    public DeleteMemberFaceCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ILogger<DeleteMemberFaceCommandHandler> logger, IFaceApiService faceApiService, IMessageBus messageBus)
    {
        _context = context;
        _authorizationService = authorizationService;
        _logger = logger;
        _faceApiService = faceApiService;
        _messageBus = messageBus;
    }

    public async Task<Result<Unit>> Handle(DeleteMemberFaceCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MemberFaces
            .Include(mf => mf.Member) // Include Member to get FamilyId for authorization
            .FirstOrDefaultAsync(mf => mf.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<Unit>.Failure($"MemberFace with ID {request.Id} not found.", ErrorSources.NotFound);
        }

        // Authorization: Check if user can access the family this memberFace belongs to
        if (entity.Member == null || !_authorizationService.CanAccessFamily(entity.Member.FamilyId))
        {
            return Result<Unit>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // If the member face was synced to the vector DB, delete it from there first
        // The service DeleteFaceByIdAsync expects VectorDbId
        if (entity.VectorDbId != null) // Ensure VectorDbId exists
        {
            try
            {
                await _faceApiService.DeleteFaceByIdAsync(entity.VectorDbId);
                _logger.LogInformation("Successfully deleted MemberFace {MemberFaceId} from face API service with VectorDbId {VectorDbId}.", entity.Id, entity.VectorDbId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete MemberFace {MemberFaceId} from face API service with VectorDbId {VectorDbId}. Proceeding with local deletion.", entity.Id, entity.VectorDbId);
                // Do not re-throw; proceed with local deletion even if face API service fails
            }
        }
        else
        {
            _logger.LogWarning("MemberFace {MemberFaceId} does not have a VectorDbId. Skipping deletion from face API service.", entity.Id);
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
