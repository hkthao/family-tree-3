namespace backend.Domain.Events;

using backend.Domain.Common;

public class FaceMetadataStoredEvent(string faceId, Guid memberId) : BaseEvent
{
    public string FaceId { get; } = faceId;
    public Guid MemberId { get; } = memberId;
}
