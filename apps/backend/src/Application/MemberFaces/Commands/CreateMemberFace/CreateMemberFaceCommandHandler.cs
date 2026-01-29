using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common;
using backend.Application.MemberFaces.Messages;
using backend.Domain.Entities;
using backend.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
namespace backend.Application.MemberFaces.Commands.CreateMemberFace;

public class CreateMemberFaceCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ILogger<CreateMemberFaceCommandHandler> logger, IMessageBus messageBus) : IRequestHandler<CreateMemberFaceCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ILogger<CreateMemberFaceCommandHandler> _logger = logger;
    private readonly IMessageBus _messageBus = messageBus;
    public async Task<Result<Guid>> Handle(CreateMemberFaceCommand request, CancellationToken cancellationToken)
    {
        var member = await _context.Members.FindAsync([request.MemberId], cancellationToken);
        if (member == null)
        {
            return Result<Guid>.Failure($"Member with ID {request.MemberId} not found.", ErrorSources.NotFound);
        }
        if (!_authorizationService.CanAccessFamily(member.FamilyId))
        {
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }
        // var searchMemberFaceQuery = new SearchMemberFaceQuery(member.FamilyId)
        // {
        //     Vector = request.Embedding.Select(d => (float)d).ToList(),
        //     Limit = 1,
        // };
        // var searchQueryResult = await _mediator.Send(searchMemberFaceQuery, cancellationToken);
        // if (searchQueryResult.IsSuccess && searchQueryResult.Value != null && searchQueryResult.Value.Any())
        // {
        //     var foundFace = searchQueryResult.Value.First();
        //     if (foundFace.MemberId != request.MemberId)
        //     {
        //         _logger.LogWarning("Face with embedding found in vector DB for a different MemberId {FoundMemberId} (requested: {RequestedMemberId}). Cannot create new MemberFace.", foundFace.MemberId, request.MemberId);
        //         return Result<Guid>.Failure($"Face with similar embedding is already associated with another member in vector DB.", ErrorSources.Conflict);
        //     }
        // }
        var id = Guid.NewGuid();
        var entity = new MemberFace
        {
            Id = id,
            MemberId = request.MemberId,
            BoundingBox = new BoundingBox
            {
                X = request.BoundingBox.X,
                Y = request.BoundingBox.Y,
                Width = request.BoundingBox.Width,
                Height = request.BoundingBox.Height
            },
            Confidence = request.Confidence,
            ThumbnailUrl = request.ThumbnailUrl,
            OriginalImageUrl = request.OriginalImageUrl,
            Embedding = request.Embedding,
            Emotion = request.Emotion,
            EmotionConfidence = request.EmotionConfidence ?? 0.0,
            IsVectorDbSynced = true, // Default to false
            VectorDbId = id.ToString() // Default to null
        };
        _context.MemberFaces.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created MemberFace {MemberFaceId} for Member {MemberId}.", entity.Id, request.MemberId);

        // Fetch member to get FamilyId. This is crucial for the integration event metadata.
        // Assuming member is still available from earlier check
        // Map to FaceAddVectorRequestDto for FaceApiService
        var faceAddVectorRequest = new FaceAddVectorRequestDto
        {
            Vector = entity.Embedding, // Embedding goes to Vector property
            Metadata = new FaceMetadataDto
            {
                FamilyId = member.FamilyId.ToString(), // Get FamilyId from member
                MemberId = entity.MemberId.ToString(),
                FaceId = entity.Id.ToString(), // Use the newly generated MemberFace.Id for the DTO
                BoundingBox = new BoundingBoxDto
                {
                    X = entity.BoundingBox.X,
                    Y = entity.BoundingBox.Y,
                    Width = entity.BoundingBox.Width,
                    Height = entity.BoundingBox.Height
                },
                Confidence = (double)entity.Confidence,
                ThumbnailUrl = entity.ThumbnailUrl,
                OriginalImageUrl = entity.OriginalImageUrl,
                Emotion = entity.Emotion,
                EmotionConfidence = entity.EmotionConfidence
            }
        };

        // Create integration event
        var integrationEvent = new MemberFaceAddedMessage
        {
            FaceAddRequest = faceAddVectorRequest,
            MemberFaceLocalId = entity.Id
        };

        try
        {
            await _messageBus.PublishAsync(MessageBusConstants.Exchanges.MemberFace, MessageBusConstants.RoutingKeys.MemberFaceAdded, integrationEvent, cancellationToken);
            _logger.LogInformation("Published MemberFaceAddedMessage for MemberFace {MemberFaceId} to RabbitMQ.", entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish MemberFaceAddedMessage for MemberFace {MemberFaceId} to RabbitMQ.", entity.Id);
        }

        return Result<Guid>.Success(entity.Id);
    }
}
