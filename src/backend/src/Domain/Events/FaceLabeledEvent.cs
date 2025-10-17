namespace FamilyTree.Domain.Events;

using backend.Domain.Common;

public class FaceMetadataStoredEvent : BaseEvent
{
    public FaceMetadataStoredEvent(string faceId, Guid memberId)
    {
        FaceId = faceId;
        MemberId = memberId;
    }

    public string FaceId { get; }
    public Guid MemberId { get; }
}