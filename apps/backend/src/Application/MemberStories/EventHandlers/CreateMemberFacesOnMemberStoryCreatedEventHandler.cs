using backend.Application.MemberFaces.Commands.CreateMemberFace;
using backend.Application.MemberFaces.Common;
using backend.Domain.Events.MemberStories;
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
        var facesData = notification.FacesData;
        foreach (var faceData in facesData)
        {
            var createFaceCommand = new CreateMemberFaceCommand
            {
                MemberId = faceData.MemberId,
                FaceId = faceData.Id,
                BoundingBox = new BoundingBoxDto { X = (int)faceData.BoundingBox.X, Y = (int)faceData.BoundingBox.Y, Width = (int)faceData.BoundingBox.Width, Height = (int)faceData.BoundingBox.Height },
                Confidence = (float)faceData.Confidence,
                OriginalImageUrl = memberStory.OriginalImageUrl,
                Embedding = faceData.Embedding.ToList(),
                Thumbnail = faceData.Thumbnail,
                Emotion = faceData.Emotion,
                EmotionConfidence = (float?)faceData.EmotionConfidence,
                IsVectorDbSynced = false
            };
            var createResult = await _mediator.Send(createFaceCommand, cancellationToken);
            if (!createResult.IsSuccess)
            {
                _logger.LogWarning("Failed to create face {FaceId} for Member {MemberId} during MemberStory creation: {Error}", faceData.Id, memberStory.MemberId, createResult.Error);
            }
        }

        // Publish notification for story creation
        // await _mediator.Send(new GenerateFamilyKbCommand(memberStory.Member.FamilyId.ToString(), memberStory.Id.ToString(), KbRecordType.Story), cancellationToken);
    }
}
