using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.AI.Models;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Faces.Common;
using backend.Domain.Entities;
using backend.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace backend.Application.Faces.Commands.UpsertMemberFace;

public class UpsertMemberFaceCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IN8nService n8nService, ILogger<UpsertMemberFaceCommandHandler> logger) : IRequestHandler<UpsertMemberFaceCommand, Result<UpsertMemberFaceCommandResultDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IN8nService _n8nService = n8nService;
    private readonly ILogger<UpsertMemberFaceCommandHandler> _logger = logger;

    public async Task<Result<UpsertMemberFaceCommandResultDto>> Handle(UpsertMemberFaceCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate Member
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken);

        if (member == null)
        {
            return Result<UpsertMemberFaceCommandResultDto>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId} not found."), ErrorSources.NotFound);
        }

        // 2. Authorize
        if (!_authorizationService.CanAccessFamily(request.FamilyId))
        {
            return Result<UpsertMemberFaceCommandResultDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // 3. Perform n8n vector search for existing face
        var searchFaceVectorDto = new SearchFaceVectorOperationDto
        {
            Vector = request.Embedding.Select(d => (float)d).ToList(),
            Limit = 1, // We only need one match to check for existence
            Threshold = 0.95f, // High threshold for exact match
            Filter = new Dictionary<string, object>
            {
                { "faceId", request.FaceId }, // Search specifically for this FaceId
                { "familyId", request.FamilyId.ToString() } // NEW: Filter by family ID
            },
            ReturnFields = new List<string> { "localDbId", "memberId" }
        };

        var n8nSearchResult = await _n8nService.CallSearchFaceVectorWebhookAsync(searchFaceVectorDto, cancellationToken);

        if (!n8nSearchResult.IsSuccess || n8nSearchResult.Value == null || !n8nSearchResult.Value.Success)
        {
            _logger.LogWarning("Failed to search face vector for FaceId {FaceId} in n8n during upsert: {Error}. Proceeding with new face creation.", request.FaceId, n8nSearchResult.Error ?? n8nSearchResult.Value?.Message);
            // If search fails or returns no results, treat as new face
        }
        else if (n8nSearchResult.Value.SearchResults != null && n8nSearchResult.Value.SearchResults.Count != 0)
        {
            var foundVector = n8nSearchResult.Value.SearchResults.First();
            if (foundVector.Payload != null && foundVector.Payload.TryGetValue("memberId", out var memberIdObj) && Guid.TryParse(memberIdObj?.ToString(), out var foundMemberId))
            {
                if (foundMemberId != request.MemberId)
                {
                    // Conflict: Face found, but associated with a different member
                    _logger.LogWarning("Face with FaceId {FaceId} found in vector DB, but associated with different MemberId {FoundMemberId} (requested: {RequestedMemberId}).", request.FaceId, foundMemberId, request.MemberId);
                    return Result<UpsertMemberFaceCommandResultDto>.Failure($"Face with ID {request.FaceId} is already associated with another member.", ErrorSources.Conflict);
                }
                else
                {
                    // Face found and associated with the same member, skip saving (per user instruction)
                    _logger.LogInformation("Face with FaceId {FaceId} already exists in vector DB and is associated with MemberId {MemberId}. Skipping upsert.", request.FaceId, request.MemberId);

                    // Retrieve local MemberFace and return its ID
                    var existingLocalMemberFace = await _context.MemberFaces.FirstOrDefaultAsync(mf => mf.FaceId == request.FaceId && mf.MemberId == request.MemberId, cancellationToken);
                    if (existingLocalMemberFace != null)
                    {
                        return Result<UpsertMemberFaceCommandResultDto>.Success(new UpsertMemberFaceCommandResultDto
                        {
                            VectorDbId = existingLocalMemberFace.VectorDbId ?? "" // Return existing VectorDbId
                        });
                    }
                    else
                    {
                        // If for some reason local entity doesn't exist, create it locally
                        _logger.LogWarning("Vector DB reports existing face {FaceId} for MemberId {MemberId}, but local MemberFace entity not found. Creating local entity.", request.FaceId, request.MemberId);
                        // Fall through to new face creation below, which will also upsert to vector DB.
                    }
                }
            }
        }

        // 4. If no conflict and not skipped, proceed to create/update local MemberFace and upsert to vector DB
        MemberFace? memberFace = null;
        var vectorDbId = Guid.NewGuid();;

        if (memberFace == null)
        {
            memberFace = new MemberFace
            {
                Id = Guid.NewGuid(), // Generate new ID for local entity
                MemberId = request.MemberId,
                FaceId = request.FaceId,
                BoundingBox = new BoundingBox { X = request.BoundingBox.X, Y = request.BoundingBox.Y, Width = request.BoundingBox.Width, Height = request.BoundingBox.Height },
                Confidence = request.Confidence,
                ThumbnailUrl = request.ThumbnailUrl,
                OriginalImageUrl = request.OriginalImageUrl,
                Embedding = request.Embedding,
                Emotion = request.Emotion,
                EmotionConfidence = request.EmotionConfidence,
                IsVectorDbSynced = false,
                VectorDbId = vectorDbId.ToString() 
            };
            _context.MemberFaces.Add(memberFace);
            _logger.LogInformation("Created new MemberFace entity for MemberId {MemberId} and FaceId {FaceId}.", request.MemberId, request.FaceId);
        }

        // 5. Call n8n Face Vector Webhook for Upsert (if not skipped)
        var upsertFaceVectorDto = new UpsertFaceVectorOperationDto
        {
            Id = vectorDbId,
            Vector = request.Embedding.Select(d => (float)d).ToList(), // Convert double to float
            Payload = new Dictionary<string, object>
            {
                { "localDbId", memberFace.Id.ToString() }, // ID of the local MemberFace entity
                { "memberId", request.MemberId.ToString() },
                { "faceId", request.FaceId },
                { "thumbnailUrl", request.ThumbnailUrl ?? "" },
                { "originalImageUrl", request.OriginalImageUrl ?? "" },
                { "emotion", request.Emotion ?? "" },
                { "emotionConfidence", request.EmotionConfidence },
                { "vectorDbId", vectorDbId }
            }
        };

        var n8nUpsertResult = await _n8nService.CallUpsertFaceVectorWebhookAsync(upsertFaceVectorDto, cancellationToken);
        if (!n8nUpsertResult.IsSuccess || n8nUpsertResult.Value == null || !n8nUpsertResult.Value.Success)
        {
            _logger.LogError("Failed to upsert face vector for MemberFaceId {MemberFaceId} in n8n: {Error}", memberFace.Id, n8nUpsertResult.Error ?? n8nUpsertResult.Value?.Message);
            memberFace.IsVectorDbSynced = false; // Mark as not synced if n8n fails
        }
        else
        {
            // For newly created vectors, update VectorDbId from n8n response if available.
            // For existing ones, ensure VectorDbId is retained.
            memberFace.IsVectorDbSynced = true;
            _logger.LogInformation("Successfully upserted face vector for MemberFaceId {MemberFaceId} in n8n. VectorDbId: {VectorDbId}", memberFace.Id, memberFace.VectorDbId);
        }

        // 6. Save changes to local database
        await _context.SaveChangesAsync(cancellationToken);

        return Result<UpsertMemberFaceCommandResultDto>.Success(new UpsertMemberFaceCommandResultDto
        {
            VectorDbId = memberFace.VectorDbId ?? "" // Ensure it's not null, though it should be set if successful
        });
    }
}
