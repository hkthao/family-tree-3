using backend.Application.Common.Interfaces;
using backend.Application.MemberFaces.Common;
using backend.Application.MemberFaces.Messages;
using backend.Domain.Events.MemberFaces;
using Microsoft.Extensions.Logging;

namespace backend.Application.MemberFaces.EventHandlers;

/// <summary>
/// Xử lý sự kiện MemberFaceCreatedEvent để đăng tải MemberFaceAddedMessage lên Message Bus.
/// Điều này tách biệt trách nhiệm đăng tải sự kiện tích hợp khỏi Command Handler.
/// </summary>
public class MemberFaceCreatedEventHandler : INotificationHandler<MemberFaceCreatedEvent>
{
    private readonly ILogger<MemberFaceCreatedEventHandler> _logger;
    private readonly IApplicationDbContext _context; // Needed to get FamilyId
    private readonly IMessageBus _messageBus;

    public MemberFaceCreatedEventHandler(ILogger<MemberFaceCreatedEventHandler> logger, IApplicationDbContext context, IMessageBus messageBus)
    {
        _logger = logger;
        _context = context;
        _messageBus = messageBus;
    }

    /// <summary>
    /// Xử lý MemberFaceCreatedEvent.
    /// Tạo FaceAddVectorRequestDto và MemberFaceAddedMessage, sau đó đăng tải lên Message Bus.
    /// </summary>
    /// <param name="notification">Sự kiện MemberFaceCreatedEvent.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Một tác vụ biểu thị hoạt động xử lý.</returns>
    public async Task Handle(MemberFaceCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: {DomainEventName} received for MemberFace {MemberFaceId}.", notification.GetType().Name, notification.MemberFace.Id);

        var memberFace = notification.MemberFace;

        // Fetch member to get FamilyId. This is crucial for the integration event metadata.
        var member = await _context.Members.FindAsync([memberFace.MemberId], cancellationToken); // Changed to object[] for FindAsync
        if (member == null)
        {
            _logger.LogWarning("Member {MemberId} not found for MemberFaceCreatedEvent. Cannot publish MemberFaceAddedMessage.", memberFace.MemberId);
            return;
        }

        // Map to FaceAddVectorRequestDto for FaceApiService
        var faceAddVectorRequest = new FaceAddVectorRequestDto
        {
            Vector = memberFace.Embedding, // Embedding goes to Vector property
            Metadata = new FaceMetadataDto
            {
                FamilyId = member.FamilyId.ToString(), // Get FamilyId from member
                MemberId = memberFace.MemberId.ToString(),
                FaceId = memberFace.Id.ToString(), // Use the newly generated MemberFace.Id for the DTO
                BoundingBox = new BoundingBoxDto
                {
                    X = memberFace.BoundingBox.X,
                    Y = memberFace.BoundingBox.Y,
                    Width = memberFace.BoundingBox.Width,
                    Height = memberFace.BoundingBox.Height
                },
                Confidence = (double)memberFace.Confidence,
                ThumbnailUrl = memberFace.ThumbnailUrl,
                OriginalImageUrl = memberFace.OriginalImageUrl,
                Emotion = memberFace.Emotion,
                EmotionConfidence = memberFace.EmotionConfidence
            }
        };

        // Create integration event
        var integrationEvent = new MemberFaceAddedMessage
        {
            FaceAddRequest = faceAddVectorRequest,
            MemberFaceLocalId = memberFace.Id
        };

        try
        {
            await _messageBus.PublishAsync("face_exchange", "face.add", integrationEvent, cancellationToken);
            _logger.LogInformation("Published MemberFaceAddedMessage for MemberFace {MemberFaceId} to RabbitMQ.", memberFace.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish MemberFaceAddedMessage for MemberFace {MemberFaceId} to RabbitMQ.", memberFace.Id);
        }
    }
}
