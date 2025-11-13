using backend.Domain.Entities;

namespace backend.Domain.Events.Members;

public class MemberCreatedEvent : BaseEvent, IDomainEvent
{
    public MemberCreatedEvent(Member member)
    {
        Member = member;
    }

    public Member Member { get; }
}
