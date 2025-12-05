using backend.Application.AI.Models;
using backend.Application.Common.Interfaces;
using backend.Domain.Events.MemberFaces;
using Microsoft.Extensions.Logging;

namespace backend.Application.MemberFaces.EventHandlers;

public class MemberFaceVectorDbSyncOnCreateHandler : INotificationHandler<MemberFaceCreatedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IN8nService _n8nService;
    private readonly ILogger<MemberFaceVectorDbSyncOnCreateHandler> _logger;

    public MemberFaceVectorDbSyncOnCreateHandler(IApplicationDbContext context, IN8nService n8nService, ILogger<MemberFaceVectorDbSyncOnCreateHandler> logger)
    {
        _context = context;
        _n8nService = n8nService;
        _logger = logger;
    }

    public async Task Handle(MemberFaceCreatedEvent notification, CancellationToken cancellationToken)
    {
        var memberFace = notification.MemberFace;
        var member = await _context.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == memberFace.MemberId, cancellationToken);
        if (member == null)
        {
            _logger.LogWarning("Member not found for MemberFaceId {MemberFaceId}. Cannot sync familyId to vector DB.", memberFace.Id);
            return; // Exit if member is not found
        }

        string? effectiveThumbnailUrl = memberFace.ThumbnailUrl;
        var upsertFaceVectorDto = new UpsertFaceVectorOperationDto
        {
            Id = Guid.TryParse(memberFace.VectorDbId, out var vectorDbId) ? vectorDbId : Guid.NewGuid(),
            Vector = [.. memberFace.Embedding.Select(d => (float)d)], // Convert double to float
            Payload = new Dictionary<string, object>
            {
                { "localDbId", memberFace.Id.ToString() },
                { "memberId", memberFace.MemberId.ToString() },
                { "familyId", member.FamilyId.ToString() }, // Add FamilyId to payload
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
            _logger.LogError("Failed to upsert face vector for MemberFaceId {MemberFaceId} in n8n (event handler): {Error}", memberFace.Id, n8nUpsertResult.Error ?? n8nUpsertResult.Value?.Message);
            memberFace.IsVectorDbSynced = false; // Mark as not synced
        }
        else
        {
            memberFace.IsVectorDbSynced = true;
            memberFace.VectorDbId = upsertFaceVectorDto.Id.ToString(); // Ensure VectorDbId is set correctly from the DTO sent to n8n
            _logger.LogInformation("Successfully upserted face vector for MemberFaceId {MemberFaceId} in n8n (event handler). VectorDbId: {VectorDbId}", memberFace.Id, memberFace.VectorDbId);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
