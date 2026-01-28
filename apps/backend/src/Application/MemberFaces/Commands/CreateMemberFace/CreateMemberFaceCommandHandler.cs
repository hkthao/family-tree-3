using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common;
using backend.Application.MemberFaces.Queries.SearchVectorFace;
using backend.Domain.Entities;
using backend.Domain.Events.MemberFaces;
using backend.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
namespace backend.Application.MemberFaces.Commands.CreateMemberFace;

public class CreateMemberFaceCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ILogger<CreateMemberFaceCommandHandler> logger, IMediator mediator, IFaceApiService faceApiService) : IRequestHandler<CreateMemberFaceCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ILogger<CreateMemberFaceCommandHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IFaceApiService _faceApiService = faceApiService;
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
        var searchMemberFaceQuery = new SearchMemberFaceQuery(member.FamilyId)
        {
            Vector = [.. request.Embedding],
            Limit = 1,
            Threshold = 0.95f
        };
        var searchQueryResult = await _mediator.Send(searchMemberFaceQuery, cancellationToken);
        if (searchQueryResult.IsSuccess && searchQueryResult.Value != null && searchQueryResult.Value.Any())
        {
            var foundFace = searchQueryResult.Value.First();
            if (foundFace.MemberId != request.MemberId)
            {
                _logger.LogWarning("Face with embedding found in vector DB for a different MemberId {FoundMemberId} (requested: {RequestedMemberId}). Cannot create new MemberFace.", foundFace.MemberId, request.MemberId);
                return Result<Guid>.Failure($"Face with similar embedding is already associated with another member in vector DB.", ErrorSources.Conflict);
            }
        }
        var entity = new MemberFace
        {
            Id = Guid.NewGuid(),
            MemberId = request.MemberId,
            FaceId = request.FaceId,
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
            IsVectorDbSynced = false, // Default to false
            VectorDbId = null // Default to null
        };
        // Map to FaceAddVectorRequestDto for FaceApiService
        var faceAddVectorRequest = new FaceAddVectorRequestDto
        {
            Vector = request.Embedding, // Embedding goes to Vector property
            Metadata = new FaceMetadataDto
            {
                FamilyId = member.FamilyId.ToString(), // Get FamilyId from member
                MemberId = request.MemberId.ToString(),
                FaceId = entity.Id.ToString(), // Use the newly generated MemberFace.Id for the DTO
                BoundingBox = new BoundingBoxDto
                {
                    X = request.BoundingBox.X,
                    Y = request.BoundingBox.Y,
                    Width = request.BoundingBox.Width,
                    Height = request.BoundingBox.Height
                },
                Confidence = (double)request.Confidence,
                ThumbnailUrl = request.ThumbnailUrl,
                OriginalImageUrl = request.OriginalImageUrl,
                Emotion = request.Emotion,
                EmotionConfidence = request.EmotionConfidence ?? 0.0
            }
        };
        try
        {
            // Call AddFaceByVectorAsync and expect VectorDbId in return
            var result = await _faceApiService.AddFaceByVectorAsync(faceAddVectorRequest);
            if (result != null && result.TryGetValue("vector_db_id", out var vectorDbIdObject) && vectorDbIdObject is string vecDbId)
            {
                entity.MarkAsVectorDbSynced(vecDbId);
                _logger.LogInformation("Successfully indexed MemberFace {MemberFaceId} to face API service. Returned VectorDbId: {VectorDbId}", entity.Id, entity.VectorDbId);
            }
            else
            {
                _logger.LogError("Failed to get VectorDbId from face API service for MemberFace {MemberFaceId}. MemberFace will be created but not synced.", entity.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index MemberFace {MemberFaceId} to face API service: {Error}. MemberFace will be created but not synced.", entity.Id, ex.Message);
        }
        _context.MemberFaces.Add(entity);
        entity.AddDomainEvent(new MemberFaceCreatedEvent(entity));
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created MemberFace {MemberFaceId} for Member {MemberId}.", entity.Id, request.MemberId);
        return Result<Guid>.Success(entity.Id);
    }
}
