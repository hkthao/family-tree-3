using backend.Domain.Entities;

namespace backend.Domain.Events.Members;

public class MemberUpdatedEvent : BaseEvent, IDomainEvent
{
    public MemberUpdatedEvent(Member member)
    {
        Member = member;
    }

    public Member Member { get; }
}
