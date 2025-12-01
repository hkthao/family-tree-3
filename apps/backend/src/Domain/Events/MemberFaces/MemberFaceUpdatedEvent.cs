using backend.Domain.Entities;

namespace backend.Domain.Events.MemberFaces;

public class MemberFaceUpdatedEvent : BaseEvent
{
    public MemberFace MemberFace { get; }

    public MemberFaceUpdatedEvent(MemberFace memberFace)
    {
        MemberFace = memberFace;
    }
}
