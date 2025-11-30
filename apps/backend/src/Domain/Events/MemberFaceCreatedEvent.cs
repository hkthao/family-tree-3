using backend.Domain.Entities;

namespace backend.Domain.Events;

public class MemberFaceCreatedEvent : BaseEvent
{
    public MemberFace MemberFace { get; }

    public MemberFaceCreatedEvent(MemberFace memberFace)
    {
        MemberFace = memberFace;
    }
}
