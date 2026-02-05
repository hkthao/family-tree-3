using backend.Application.Common.Constants;
using backend.Application.Common.Utils; // NEW
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia; // NEW
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common;
using backend.Application.MemberFaces.Messages;
using backend.Domain.Entities;
using backend.Domain.Enums; // NEW
using backend.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Interfaces.Services;
namespace backend.Application.MemberFaces.Commands.CreateMemberFace;

public class CreateMemberFaceCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ILogger<CreateMemberFaceCommandHandler> logger, IMessageBus messageBus, IMediator mediator) : IRequestHandler<CreateMemberFaceCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ILogger<CreateMemberFaceCommandHandler> _logger = logger;
    private readonly IMessageBus _messageBus = messageBus;
    private readonly IMediator _mediator = mediator;
    public async Task<Result<Guid>> Handle(CreateMemberFaceCommand request, CancellationToken cancellationToken)
    {
        var member = await _context.Members.FindAsync([request.MemberId], cancellationToken);
        if (member == null)
        {
            return Result<Guid>.Failure($"Member with ID {request.MemberId} not found.", ErrorSources.NotFound);
        }
        if (!_authorizationService.CanAccessFamily(member.FamilyId))
        {
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }
        var id = Guid.NewGuid();

        string? finalThumbnailUrl = null; // Declare here
        if (!string.IsNullOrEmpty(request.Thumbnail))
        {
            try
            {
                var imageData = ImageUtils.ConvertBase64ToBytes(request.Thumbnail);
                var contentType = ImageUtils.GetMimeTypeFromBase64(request.Thumbnail);

                var createFamilyMediaCommand = new CreateFamilyMediaCommand
                {
                    FamilyId = member.FamilyId,
                    RefId = id, // Link media to the Member
                    RefType = RefType.MemberFace, // Thumbnail for a MemberFace
                    MediaLinkType = MediaLinkType.Thumbnail, // Specify it's a thumbnail
                    AllowMultipleMediaLinks = true, // Allow multiple thumbnails per member (different faces)
                    File = imageData,
                    FileName = $"MemberFace_Thumbnail_{Guid.NewGuid()}.png",
                    ContentType = contentType,
                    Folder = string.Format(UploadConstants.FaceImagesFolder, member.FamilyId),
                    MediaType = MediaType.Image // Explicitly set MediaType
                };

                var uploadResult = await _mediator.Send(createFamilyMediaCommand, cancellationToken);

                if (!uploadResult.IsSuccess)
                {
                    _logger.LogError("Failed to create FamilyMedia for MemberFace thumbnail: {Error}", uploadResult.Error);
                    // Depending on desired behavior, could fail the command or proceed without thumbnail
                    // For now, we'll return failure as thumbnail is important for face.
                    return Result<Guid>.Failure(string.Format(ErrorMessages.FileUploadFailed, uploadResult.Error), ErrorSources.FileUpload);
                }

                if (uploadResult.Value == null || string.IsNullOrEmpty(uploadResult.Value.FilePath))
                {
                    _logger.LogError("FamilyMedia creation for MemberFace thumbnail returned null or empty FilePath.");
                    return Result<Guid>.Failure(ErrorMessages.FileUploadNullUrl, ErrorSources.FileUpload);
                }
                finalThumbnailUrl = uploadResult.Value.FilePath; // Correctly assign the value

            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Invalid Base64 format for MemberFace thumbnail.");
                return Result<Guid>.Failure(ErrorMessages.InvalidBase64, ErrorSources.Validation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MemberFace thumbnail: {Message}", ex.Message);
                return Result<Guid>.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Exception);
            }
        }

        var entity = new MemberFace
        {
            Id = id,
            MemberId = request.MemberId,
            BoundingBox = new BoundingBox
            {
                X = request.BoundingBox.X,
                Y = request.BoundingBox.Y,
                Width = request.BoundingBox.Width,
                Height = request.BoundingBox.Height
            },
            Confidence = request.Confidence,
            ThumbnailUrl = finalThumbnailUrl,
            Embedding = request.Embedding,
            Emotion = request.Emotion,
            EmotionConfidence = request.EmotionConfidence ?? 0.0,
            IsVectorDbSynced = false, // Set to false as the property is being removed from the command
            VectorDbId = id.ToString() // Default to null
        };
        _context.MemberFaces.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created MemberFace {MemberFaceId} for Member {MemberId}.", entity.Id, request.MemberId);

        // Fetch member to get FamilyId. This is crucial for the integration event metadata.
        // Assuming member is still available from earlier check
        // Map to FaceAddVectorRequestDto for FaceApiService
        var faceAddVectorRequest = new FaceAddVectorRequestDto
        {
            Vector = entity.Embedding, // Embedding goes to Vector property
            Metadata = new FaceMetadataDto
            {
                FamilyId = member.FamilyId.ToString(), // Get FamilyId from member
                MemberId = entity.MemberId.ToString(),
                FaceId = entity.Id.ToString(), // Use the newly generated MemberFace.Id for the DTO
                BoundingBox = new BoundingBoxDto
                {
                    X = entity.BoundingBox.X,
                    Y = entity.BoundingBox.Y,
                    Width = entity.BoundingBox.Width,
                    Height = entity.BoundingBox.Height
                },
                Confidence = (double)entity.Confidence,
                Emotion = entity.Emotion,
                EmotionConfidence = entity.EmotionConfidence
            }
        };

        // Create integration event
        var integrationEvent = new MemberFaceAddedMessage
        {
            FaceAddRequest = faceAddVectorRequest,
            MemberFaceLocalId = entity.Id
        };

        try
        {
            await _messageBus.PublishAsync(MessageBusConstants.Exchanges.MemberFace, MessageBusConstants.RoutingKeys.MemberFaceAdded, integrationEvent, cancellationToken);
            _logger.LogInformation("Published MemberFaceAddedMessage for MemberFace {MemberFaceId} to RabbitMQ.", entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish MemberFaceAddedMessage for MemberFace {MemberFaceId} to RabbitMQ.", entity.Id);
        }

        return Result<Guid>.Success(entity.Id);
    }
}
