using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Interfaces.Services;
using backend.Application.Common.Models;
using backend.Application.Common.Models.MessageBus; // NEW
using Microsoft.Extensions.Logging;

namespace backend.Application.FamilyMedias.Commands.DeleteFamilyMedia;

public class DeleteFamilyMediaCommandHandler : IRequestHandler<DeleteFamilyMediaCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMessageBus _messageBus; // NEW
    private readonly ILogger<DeleteFamilyMediaCommandHandler> _logger;

    public DeleteFamilyMediaCommandHandler(
        IApplicationDbContext context,
        IAuthorizationService authorizationService,
        IMessageBus messageBus, // NEW
        ILogger<DeleteFamilyMediaCommandHandler> logger)
    {
        _context = context;
        _authorizationService = authorizationService;
        _messageBus = messageBus; // NEW
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteFamilyMediaCommand request, CancellationToken cancellationToken)
    {

        var familyMedia = await _context.FamilyMedia
            .FirstOrDefaultAsync(fm => fm.Id == request.Id && !fm.IsDeleted, cancellationToken);

        if (familyMedia == null)
        {
            return Result.Failure(string.Format(ErrorMessages.NotFound, $"FamilyMedia with ID {request.Id}"), ErrorSources.NotFound);
        }

        // Authorization check - only applies if media is associated with a family
        if (familyMedia.FamilyId.HasValue && !_authorizationService.CanManageFamily(familyMedia.FamilyId.Value))
        {
            return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // Publish message to RabbitMQ for storage-service to handle deletion
        var fileDeletionEvent = new FileDeletionRequestedEvent
        {
            FileId = familyMedia.Id,
            FilePath = familyMedia.FilePath,
            DeleteHash = familyMedia.DeleteHash, // Pass DeleteHash if available
            RequestedBy = Guid.Empty, // UploadedBy property was removed. CreatedBy is a string, so using Guid.Empty for RequestedBy (Guid).
            FamilyId = familyMedia.FamilyId
        };
        await _messageBus.PublishAsync(MessageBusConstants.Exchanges.FileUpload, MessageBusConstants.RoutingKeys.FileDeletionRequested, fileDeletionEvent);

        _logger.LogInformation("FileDeletionRequestedEvent published for FileId {FileId} with FilePath {FilePath}", familyMedia.Id, familyMedia.FilePath);

        _context.FamilyMedia.Remove(familyMedia); // FamilyMedia is now hard deleted

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
