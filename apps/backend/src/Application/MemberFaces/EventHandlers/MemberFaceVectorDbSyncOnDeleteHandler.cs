using backend.Application.AI.Models;
using backend.Application.Common.Interfaces;
using backend.Domain.Events.MemberFaces;
using Microsoft.Extensions.Logging;

namespace backend.Application.MemberFaces.EventHandlers;

public class MemberFaceVectorDbSyncOnDeleteHandler : INotificationHandler<MemberFaceDeletedEvent>
{
    private readonly IN8nService _n8nService;
    private readonly ILogger<MemberFaceVectorDbSyncOnDeleteHandler> _logger;

    public MemberFaceVectorDbSyncOnDeleteHandler(IN8nService n8nService, ILogger<MemberFaceVectorDbSyncOnDeleteHandler> logger)
    {
        _n8nService = n8nService;
        _logger = logger;
    }

    public async Task Handle(MemberFaceDeletedEvent notification, CancellationToken cancellationToken)
    {
        var memberFaceId = notification.MemberFace.Id;
        var vectorDbId = notification.MemberFace.VectorDbId;

        if (string.IsNullOrEmpty(vectorDbId))
        {
            _logger.LogWarning("MemberFaceId {MemberFaceId} does not have a valid VectorDbId. Skipping vector DB deletion.", memberFaceId);
            return;
        }

        var deleteFaceVectorDto = new DeleteFaceVectorOperationDto
        {
            PointIds = new List<string> { vectorDbId }
        };

        var n8nDeleteResult = await _n8nService.CallDeleteFaceVectorWebhookAsync(deleteFaceVectorDto, cancellationToken);

        if (!n8nDeleteResult.IsSuccess || n8nDeleteResult.Value == null || !n8nDeleteResult.Value.Success)
        {
            _logger.LogError("Failed to delete face vector for MemberFaceId {MemberFaceId} (VectorDbId: {VectorDbId}) in n8n (on delete event handler): {Error}", memberFaceId, vectorDbId, n8nDeleteResult.Error ?? n8nDeleteResult.Value?.Message);
            // Optionally, mark local entity as not synced if it's a soft delete and we need to retry
        }
        else
        {
            _logger.LogInformation("Successfully deleted face vector for MemberFaceId {MemberFaceId} (VectorDbId: {VectorDbId}) in n8n (on delete event handler).", memberFaceId, vectorDbId);
            // If it's a soft delete, update the local entity to reflect successful vector DB deletion.
            // For a hard delete, the local entity is already gone.
        }
    }
}
