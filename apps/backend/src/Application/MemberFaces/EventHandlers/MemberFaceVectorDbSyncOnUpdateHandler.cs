using backend.Application.AI.Models;
using backend.Application.Common.Interfaces;
using backend.Domain.Events.MemberFaces;
using Microsoft.Extensions.Logging;

namespace backend.Application.MemberFaces.EventHandlers;

public class MemberFaceVectorDbSyncOnUpdateHandler : INotificationHandler<MemberFaceUpdatedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IN8nService _n8nService;
    private readonly ILogger<MemberFaceVectorDbSyncOnUpdateHandler> _logger;

    public MemberFaceVectorDbSyncOnUpdateHandler(IApplicationDbContext context, IN8nService n8nService, ILogger<MemberFaceVectorDbSyncOnUpdateHandler> logger)
    {
        _context = context;
        _n8nService = n8nService;
        _logger = logger;
    }

    public async Task Handle(MemberFaceUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var memberFace = notification.MemberFace;

        // Load the Member navigation property explicitly if not already loaded
        // This is crucial to access memberFace.Member.FamilyId
        if (memberFace.Member == null)
        {
            memberFace = await _context.MemberFaces
                .Include(mf => mf.Member)
                .FirstOrDefaultAsync(mf => mf.Id == memberFace.Id, cancellationToken);

            if (memberFace == null)
            {
                _logger.LogWarning("MemberFace with ID {MemberFaceId} not found when trying to load Member. Skipping vector DB sync.", notification.MemberFace.Id);
                return;
            }
        }
        
        // Ensure thumbnail is available for payload, if not, try to fetch from ThumbnailUrl
        string? effectiveThumbnailUrl = memberFace.ThumbnailUrl; // Assuming ThumbnailUrl is updated in the MemberFace entity

        var upsertFaceVectorDto = new UpsertFaceVectorOperationDto
        {
            Id = Guid.TryParse(memberFace.VectorDbId, out var vectorDbId) ? vectorDbId : Guid.NewGuid(), // Use existing VectorDbId or generate if missing
            Vector = [.. memberFace.Embedding.Select(d => (float)d)], // Convert double to float
            Payload = new Dictionary<string, object>
            {
                { "localDbId", memberFace.Id.ToString() }, // ID of the local MemberFace entity
                { "memberId", memberFace.MemberId.ToString() },
                { "familyId", memberFace.Member.FamilyId.ToString() }, // Thêm FamilyId
                { "faceId", memberFace.FaceId },
                { "thumbnailUrl", effectiveThumbnailUrl ?? "" },
                { "originalImageUrl", memberFace.OriginalImageUrl ?? "" },
                { "emotion", memberFace.Emotion ?? "" },
                { "emotionConfidence", memberFace.EmotionConfidence },
                { "vectorDbId", memberFace.VectorDbId ?? Guid.NewGuid().ToString() } // Use existing or generate new
            }
        };

        var n8nUpsertResult = await _n8nService.CallUpsertFaceVectorWebhookAsync(upsertFaceVectorDto, cancellationToken);

        if (!n8nUpsertResult.IsSuccess || n8nUpsertResult.Value == null || !n8nUpsertResult.Value.Success)
        {
            _logger.LogError("Failed to upsert face vector for MemberFaceId {MemberFaceId} in n8n (on update event handler): {Error}", memberFace.Id, n8nUpsertResult.Error ?? n8nUpsertResult.Value?.Message);
            memberFace.IsVectorDbSynced = false; // Mark as not synced
        }
        else
        {
            memberFace.IsVectorDbSynced = true;
            memberFace.VectorDbId = upsertFaceVectorDto.Id.ToString(); // Ensure VectorDbId is set correctly from the DTO sent to n8n
            _logger.LogInformation("Successfully upserted face vector for MemberFaceId {MemberFaceId} in n8n (on update event handler). VectorDbId: {VectorDbId}", memberFace.Id, memberFace.VectorDbId);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
