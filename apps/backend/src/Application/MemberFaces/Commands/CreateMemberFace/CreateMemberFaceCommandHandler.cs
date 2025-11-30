using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using backend.Domain.ValueObjects;
using backend.Application.Common.Constants;
using backend.Application.Faces.Queries; // NEW: For SearchMemberFaceQuery
using backend.Domain.Events; // NEW: For MemberFaceCreatedEvent

namespace backend.Application.MemberFaces.Commands.CreateMemberFace;

// NEW: Inject IMediator and IThumbnailUploadService
public class CreateMemberFaceCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ILogger<CreateMemberFaceCommandHandler> logger, IMediator mediator, IThumbnailUploadService thumbnailUploadService) : IRequestHandler<CreateMemberFaceCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ILogger<CreateMemberFaceCommandHandler> _logger = logger;
    private readonly IMediator _mediator = mediator; // NEW: Field for IMediator
    private readonly IThumbnailUploadService _thumbnailUploadService = thumbnailUploadService; // NEW: Field for IThumbnailUploadService

    public async Task<Result<Guid>> Handle(CreateMemberFaceCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate Member
        var member = await _context.Members.FindAsync(new object[] { request.MemberId }, cancellationToken);
        if (member == null)
        {
            return Result<Guid>.Failure($"Member with ID {request.MemberId} not found.", ErrorSources.NotFound);
        }

        // 2. Authorize
        if (!_authorizationService.CanAccessFamily(member.FamilyId))
        {
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        string? thumbnailUrl = null;
        // 3. Thumbnail Upload Logic
        if (!string.IsNullOrEmpty(request.Thumbnail))
        {
            var uploadResult = await _thumbnailUploadService.UploadThumbnailAsync(request.Thumbnail, member.FamilyId, request.FaceId, cancellationToken);
            if (uploadResult.IsSuccess)
            {
                thumbnailUrl = uploadResult.Value;
            }
            else
            {
                _logger.LogWarning("Failed to upload face thumbnail using ThumbnailUploadService for FaceId {FaceId}: {Error}", request.FaceId, uploadResult.Error);
                // Proceed without thumbnail, but log warning.
            }
        }

        // 4. Conflict Detection Logic (Vector Search)
        // Perform a vector search to ensure this face embedding isn't already used by a different member.
        var searchMemberFaceQuery = new SearchMemberFaceQuery
        {
            Vector = request.Embedding.ToList(),
            Limit = 1,
            Threshold = 0.95f // Use the same threshold as in UpsertMemberFaceCommandHandler
        };
        var searchQueryResult = await _mediator.Send(searchMemberFaceQuery, cancellationToken);

        if (searchQueryResult.IsSuccess && searchQueryResult.Value != null && searchQueryResult.Value.Any())
        {
            var foundFace = searchQueryResult.Value.First();
            // If a vector is found for this embedding, but it's for a different member, then it's a conflict.
            if (foundFace.MemberId != request.MemberId)
            {
                _logger.LogWarning("Face with embedding found in vector DB for a different MemberId {FoundMemberId} (requested: {RequestedMemberId}). Cannot create new MemberFace.", foundFace.MemberId, request.MemberId);
                return Result<Guid>.Failure($"Face with similar embedding is already associated with another member in vector DB.", ErrorSources.Conflict);
            }
        }

        // 5. Create MemberFace entity
        var entity = new MemberFace
        {
            Id = Guid.NewGuid(), // Generate new ID
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
            ThumbnailUrl = thumbnailUrl, // Use uploaded URL
            OriginalImageUrl = request.OriginalImageUrl,
            Embedding = request.Embedding,
            Emotion = request.Emotion,
            EmotionConfidence = request.EmotionConfidence ?? 0.0,
            IsVectorDbSynced = false, // Will be set by event handler
            VectorDbId = null // Will be set by event handler
        };

        _context.MemberFaces.Add(entity);

        // 6. Emit MemberFaceCreatedEvent
        entity.AddDomainEvent(new MemberFaceCreatedEvent(entity)); // NEW: Add domain event

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created MemberFace {MemberFaceId} for Member {MemberId}.", entity.Id, request.MemberId);

        return Result<Guid>.Success(entity.Id);
    }
}
