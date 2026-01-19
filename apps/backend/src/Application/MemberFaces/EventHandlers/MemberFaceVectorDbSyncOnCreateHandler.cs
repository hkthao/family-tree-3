using backend.Application.Common.Interfaces;
using backend.Domain.Events.MemberFaces;
using Microsoft.Extensions.Logging;
using backend.Application.Knowledge; // Added for IKnowledgeService
using backend.Application.Knowledge.DTOs; // Added for MemberFaceDto
using backend.Domain.ValueObjects; // Ensure BoundingBox is recognized
namespace backend.Application.MemberFaces.EventHandlers;
public class MemberFaceVectorDbSyncOnCreateHandler : INotificationHandler<MemberFaceCreatedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IKnowledgeService _knowledgeService; // Changed from IN8nService
    private readonly ILogger<MemberFaceVectorDbSyncOnCreateHandler> _logger;
    public MemberFaceVectorDbSyncOnCreateHandler(IApplicationDbContext context, IKnowledgeService knowledgeService, ILogger<MemberFaceVectorDbSyncOnCreateHandler> logger) // Changed from IN8nService
    {
        _context = context;
        _knowledgeService = knowledgeService; // Changed from IN8nService
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
        // Generate VectorDbId here in the backend
        var generatedVectorDbId = Guid.NewGuid().ToString();
        _logger.LogDebug("Generated VectorDbId for MemberFaceId {MemberFaceId}: {GeneratedVectorDbId}", memberFace.Id, generatedVectorDbId);
        // Map MemberFace to MemberFaceDto for IKnowledgeService
        var memberFaceDto = new MemberFaceDto
        {
            FamilyId = member.FamilyId, // Get FamilyId from member
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
            VectorDbId = generatedVectorDbId // Use the generated VectorDbId from C#
        };
        try
        {
            // Call IndexMemberFaceData and expect VectorDbId in return
            string returnedVectorDbId = await _knowledgeService.IndexMemberFaceData(memberFaceDto);
            _logger.LogDebug("Returned VectorDbId from knowledge-search-service for MemberFaceId {MemberFaceId}: {ReturnedVectorDbId}", memberFace.Id, returnedVectorDbId);
            if (!string.IsNullOrEmpty(returnedVectorDbId))
            {
                memberFace.VectorDbId = returnedVectorDbId; // Update with the ID returned by knowledge service
                memberFace.IsVectorDbSynced = true;
                _logger.LogInformation("Successfully indexed MemberFaceId {MemberFaceId} to knowledge-search-service. Returned VectorDbId: {VectorDbId}", memberFace.Id, memberFace.VectorDbId);
            }
            else
            {
                _logger.LogError("Failed to get VectorDbId from knowledge-search-service for MemberFaceId {MemberFaceId}.", memberFace.Id);
                memberFace.IsVectorDbSynced = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index MemberFaceId {MemberFaceId} to knowledge-search-service: {Error}", memberFace.Id, ex.Message);
            memberFace.IsVectorDbSynced = false; // Mark as not synced
        }
        _logger.LogDebug("Saving MemberFaceId {MemberFaceId} with VectorDbId: {VectorDbId}", memberFace.Id, memberFace.VectorDbId);
        await _context.SaveChangesAsync(cancellationToken);    }
}
