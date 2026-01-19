using backend.Application.Common.Interfaces;
using backend.Application.Knowledge;
using backend.Domain.Events.MemberFaces;
using Microsoft.Extensions.Logging;

namespace backend.Application.MemberFaces.EventHandlers;

public class MemberFaceVectorDbSyncOnDeleteHandler : INotificationHandler<MemberFaceDeletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IKnowledgeService _knowledgeService;
    private readonly ILogger<MemberFaceVectorDbSyncOnDeleteHandler> _logger;

    public MemberFaceVectorDbSyncOnDeleteHandler(IApplicationDbContext context, IKnowledgeService knowledgeService, ILogger<MemberFaceVectorDbSyncOnDeleteHandler> logger)
    {
        _context = context;
        _knowledgeService = knowledgeService;
        _logger = logger;
    }

    public async Task Handle(MemberFaceDeletedEvent notification, CancellationToken cancellationToken)
    {
        var memberFace = notification.MemberFace;

        if (string.IsNullOrEmpty(memberFace.VectorDbId))
        {
            _logger.LogWarning("MemberFaceId {MemberFaceId} does not have a VectorDbId. Skipping deletion from knowledge-search-service.", memberFace.Id);
            return;
        }

        var member = await _context.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == memberFace.MemberId, cancellationToken);
        if (member == null)
        {
            _logger.LogWarning("Member not found for MemberFaceId {MemberFaceId}. Cannot delete from knowledge-search-service.", memberFace.Id);
            return; // Exit if member is not found
        }

        try
        {
            await _knowledgeService.DeleteMemberFaceData(member.FamilyId, Guid.Parse(memberFace.VectorDbId));
            _logger.LogInformation("Successfully deleted MemberFaceId {MemberFaceId} (VectorDbId: {VectorDbId}) from knowledge-search-service.", memberFace.Id, memberFace.VectorDbId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete MemberFaceId {MemberFaceId} (VectorDbId: {VectorDbId}) from knowledge-search-service: {Error}", memberFace.Id, memberFace.VectorDbId, ex.Message);
            // Optionally rethrow or handle specific error types
        }
    }
}