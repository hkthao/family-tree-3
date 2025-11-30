using backend.Application.MemberFaces.Commands.CreateMemberFace;
using backend.Domain.Events;
using Microsoft.Extensions.Logging;

namespace backend.Application.MemberStories.EventHandlers;

public class CreateMemberFacesOnMemberStoryCreatedEventHandler : INotificationHandler<MemberStoryCreatedWithFacesEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<CreateMemberFacesOnMemberStoryCreatedEventHandler> _logger;

    public CreateMemberFacesOnMemberStoryCreatedEventHandler(IMediator mediator, ILogger<CreateMemberFacesOnMemberStoryCreatedEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(MemberStoryCreatedWithFacesEvent notification, CancellationToken cancellationToken)
    {
        var memberStory = notification.MemberStory;
        var facesData = notification.FacesData; // Changed to FacesData

        foreach (var faceData in facesData) // Changed loop variable name
        {
            var createFaceCommand = new CreateMemberFaceCommand
            {
                MemberId = memberStory.MemberId, // Link face to the story's member
                FaceId = faceData.Id,
                BoundingBox = new backend.Application.Faces.Common.BoundingBoxDto { X = (int)faceData.BoundingBox.X, Y = (int)faceData.BoundingBox.Y, Width = (int)faceData.BoundingBox.Width, Height = (int)faceData.BoundingBox.Height }, // Cast to int
                Confidence = (float)faceData.Confidence, // Cast to float
                OriginalImageUrl = memberStory.OriginalImageUrl, // Use the story's original image URL
                Embedding = faceData.Embedding.ToList(), // Convert IReadOnlyList<double> to List<double>
                Thumbnail = faceData.Thumbnail, // Pass the base64 thumbnail
                Emotion = faceData.Emotion,
                EmotionConfidence = (float?)faceData.EmotionConfidence, // Cast to float?
                IsVectorDbSynced = false // Will be set by MemberFace event handler
            };
            var createResult = await _mediator.Send(createFaceCommand, cancellationToken);
            if (!createResult.IsSuccess)
            {
                _logger.LogWarning("Failed to create face {FaceId} for Member {MemberId} during MemberStory creation: {Error}", faceData.Id, memberStory.MemberId, createResult.Error);
                // Continue processing other faces even if one fails
            }
        }
    }
}
