using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces.Family;

public interface IMemberRelationshipService
{
    Task UpdateDenormalizedRelationshipFields(Member member, CancellationToken cancellationToken);
    Task UpdateMemberRelationshipsAsync(Guid memberId, Guid? fatherId, Guid? motherId, Guid? husbandId, Guid? wifeId, CancellationToken cancellationToken);
}
