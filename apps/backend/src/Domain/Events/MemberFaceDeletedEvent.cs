namespace backend.Domain.Events;

public class MemberFaceDeletedEvent : BaseEvent
{
    public Guid MemberFaceId { get; }
    public Guid VectorDbId { get; } // NEW: Add VectorDbId for deletion from vector DB

    public MemberFaceDeletedEvent(Guid memberFaceId, Guid vectorDbId)
    {
        MemberFaceId = memberFaceId;
        VectorDbId = vectorDbId;
    }
}
