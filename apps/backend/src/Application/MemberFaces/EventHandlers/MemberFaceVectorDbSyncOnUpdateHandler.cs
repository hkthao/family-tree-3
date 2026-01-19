using backend.Application.Common.Interfaces;
using backend.Domain.Events.MemberFaces;
using Microsoft.Extensions.Logging;
using backend.Application.Knowledge; // Added for IKnowledgeService
using backend.Application.Knowledge.DTOs; // Added for MemberFaceDto
using backend.Domain.ValueObjects; // Ensure BoundingBox is recognized
namespace backend.Application.MemberFaces.EventHandlers;
public class MemberFaceVectorDbSyncOnUpdateHandler : INotificationHandler<MemberFaceUpdatedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IKnowledgeService _knowledgeService; // Changed from IN8nService
    private readonly ILogger<MemberFaceVectorDbSyncOnUpdateHandler> _logger;
    public MemberFaceVectorDbSyncOnUpdateHandler(IApplicationDbContext context, IKnowledgeService knowledgeService, ILogger<MemberFaceVectorDbSyncOnUpdateHandler> logger) // Changed from IN8nService
    {
        _context = context;
        _knowledgeService = knowledgeService; // Changed from IN8nService
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
            if (memberFace == null || memberFace.Member == null)
            {
                _logger.LogWarning("MemberFace with ID {MemberFaceId} not found or Member navigation property is null when trying to load Member. Skipping vector DB sync.", notification.MemberFace.Id);
                return;
            }
        }
        // Use existing VectorDbId or generate a new one if missing
        var vectorDbIdToUse = memberFace.VectorDbId;
        if (string.IsNullOrEmpty(vectorDbIdToUse))
        {
            vectorDbIdToUse = Guid.NewGuid().ToString();
            _logger.LogWarning("MemberFaceId {MemberFaceId} is missing VectorDbId. Generating a new one: {VectorDbId}.", memberFace.Id, vectorDbIdToUse);
        }
        // Map MemberFace to MemberFaceDto for IKnowledgeService
        var memberFaceDto = new MemberFaceDto
        {
            FamilyId = memberFace.Member.FamilyId, // Get FamilyId from member
            MemberId = memberFace.MemberId,
            FaceId = memberFace.Id, // Use MemberFace.Id as FaceId for the DTO
            BoundingBox = new BoundingBox // Map BoundingBox
            {
                X = memberFace.BoundingBox.X,
                Y = memberFace.BoundingBox.Y,
                Width = memberFace.BoundingBox.Width,
                Height = memberFace.BoundingBox.Height
            },
            Confidence = memberFace.Confidence,
            ThumbnailUrl = memberFace.ThumbnailUrl,
            OriginalImageUrl = memberFace.OriginalImageUrl,
            Embedding = memberFace.Embedding,
            Emotion = memberFace.Emotion,
            EmotionConfidence = memberFace.EmotionConfidence,
            IsVectorDbSynced = false, // Will be set to true by knowledge service if successful
            VectorDbId = vectorDbIdToUse // Use the existing or generated VectorDbId
        };
        try
        {
            // Call IndexMemberFaceData and expect VectorDbId in return
            string returnedVectorDbId = await _knowledgeService.IndexMemberFaceData(memberFaceDto);
            if (!string.IsNullOrEmpty(returnedVectorDbId))
            {
                memberFace.VectorDbId = returnedVectorDbId; // Update with the ID returned by knowledge service
                memberFace.IsVectorDbSynced = true;
                _logger.LogInformation("Successfully updated MemberFaceId {MemberFaceId} to knowledge-search-service. Returned VectorDbId: {VectorDbId}", memberFace.Id, memberFace.VectorDbId);
            }
            else
            {
                _logger.LogError("Failed to get VectorDbId from knowledge-search-service for MemberFaceId {MemberFaceId}.", memberFace.Id);
                memberFace.IsVectorDbSynced = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update MemberFaceId {MemberFaceId} to knowledge-search-service: {Error}", memberFace.Id, ex.Message);
            memberFace.IsVectorDbSynced = false; // Mark as not synced
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}
