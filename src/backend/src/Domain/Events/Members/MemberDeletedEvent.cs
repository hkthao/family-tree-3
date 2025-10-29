using backend.Domain.Entities;

namespace backend.Domain.Events.Members;

public class MemberDeletedEvent : BaseEvent, IDomainEvent
{
    public MemberDeletedEvent(Member member)
    {
        Member = member;
    }

    public Member Member { get; }
}
