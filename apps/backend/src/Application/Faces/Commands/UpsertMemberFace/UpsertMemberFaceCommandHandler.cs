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

public class UpsertMemberFaceCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IN8nService n8nService, ILogger<UpsertMemberFaceCommandHandler> logger) : IRequestHandler<UpsertMemberFaceCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IN8nService _n8nService = n8nService;
    private readonly ILogger<UpsertMemberFaceCommandHandler> _logger = logger;

    public async Task<Result<Guid>> Handle(UpsertMemberFaceCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate Member
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken);

        if (member == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId} not found."), ErrorSources.NotFound);
        }

        // 2. Authorize
        if (!_authorizationService.CanAccessFamily(member.FamilyId))
        {
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // 3. Retrieve or Create MemberFace entity
        MemberFace? memberFace = null;
        if (!string.IsNullOrEmpty(request.VectorDbId))
        {
            memberFace = await _context.MemberFaces.FirstOrDefaultAsync(mf => mf.VectorDbId == request.VectorDbId, cancellationToken);
        }
        else
        {
            // If no VectorDbId is provided, try to find by FaceId and MemberId
            memberFace = await _context.MemberFaces
                .FirstOrDefaultAsync(mf => mf.FaceId == request.FaceId && mf.MemberId == request.MemberId, cancellationToken);
        }


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
                IsVectorDbSynced = false // Will be updated after n8n call
            };
            _context.MemberFaces.Add(memberFace);
            _logger.LogInformation("Created new MemberFace entity for MemberId {MemberId} and FaceId {FaceId}.", request.MemberId, request.FaceId);
        }
        else
        {
            memberFace.BoundingBox = new BoundingBox { X = request.BoundingBox.X, Y = request.BoundingBox.Y, Width = request.BoundingBox.Width, Height = request.BoundingBox.Height };
            memberFace.Confidence = request.Confidence;
            memberFace.ThumbnailUrl = request.ThumbnailUrl;
            memberFace.OriginalImageUrl = request.OriginalImageUrl;
            memberFace.Embedding = request.Embedding;
            memberFace.Emotion = request.Emotion;
            memberFace.EmotionConfidence = request.EmotionConfidence;
            _logger.LogInformation("Updated existing MemberFace entity with ID {MemberFaceId} for MemberId {MemberId} and FaceId {FaceId}.", memberFace.Id, request.MemberId, request.FaceId);
        }

        // 4. Call n8n Face Vector Webhook for Upsert
        var faceVectorDto = new FaceVectorOperationDto
        {
            ActionType = "upsert",
            Vector = request.Embedding.Select(d => (float)d).ToList(), // Convert double to float for FaceVectorOperationDto
            Filter = new Dictionary<string, object>
            {
                { "memberId", request.MemberId.ToString() },
                { "faceId", request.FaceId }
            },
            Payload = new Dictionary<string, object>
            {
                { "localDbId", memberFace.Id.ToString() }, // ID of the local MemberFace entity
                { "memberId", request.MemberId.ToString() },
                { "faceId", request.FaceId },
                { "thumbnailUrl", request.ThumbnailUrl ?? "" },
                { "originalImageUrl", request.OriginalImageUrl ?? "" },
                { "emotion", request.Emotion ?? "" },
                { "emotionConfidence", request.EmotionConfidence }
            }
        };

        // If updating an existing vector, include its VectorDbId in filter
        if (!string.IsNullOrEmpty(request.VectorDbId))
        {
            faceVectorDto.Filter.Add("vectorDbId", request.VectorDbId);
        }

        var n8nResult = await _n8nService.CallFaceVectorWebhookAsync(faceVectorDto, cancellationToken);

        if (!n8nResult.IsSuccess || n8nResult.Value == null || !n8nResult.Value.Success)
        {
            _logger.LogError("Failed to upsert face vector for MemberFaceId {MemberFaceId} in n8n: {Error}", memberFace.Id, n8nResult.Error ?? n8nResult.Value?.Message);
            // Even if n8n fails, we save the local entity. Sync status will be false.
        }
        else
        {
            // Update VectorDbId from n8n response if available (for newly created vectors)
            // Or ensure it's set if it was provided in the request
            if (string.IsNullOrEmpty(memberFace.VectorDbId) && n8nResult.Value.AffectedCount > 0 && n8nResult.Value.SearchResults != null && n8nResult.Value.SearchResults.Any())
            {
                memberFace.VectorDbId = n8nResult.Value.SearchResults.First().Id; // Assuming n8n returns the vector DB ID
            } else if (!string.IsNullOrEmpty(request.VectorDbId)) {
                memberFace.VectorDbId = request.VectorDbId; // Use the provided one if it was an update
            }
            memberFace.IsVectorDbSynced = true;
            _logger.LogInformation("Successfully upserted face vector for MemberFaceId {MemberFaceId} in n8n. VectorDbId: {VectorDbId}", memberFace.Id, memberFace.VectorDbId);
        }

        // 5. Save changes to local database
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(memberFace.Id);
    }
}
