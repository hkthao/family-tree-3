using backend.Domain.Common;
using backend.Domain.Entities;

namespace backend.Domain.Events.Members;

public class MemberBiographyUpdatedEvent : BaseEvent
{
    public MemberBiographyUpdatedEvent(Member member)
    {
        Member = member;
    }

    public Member Member { get; }
}
