using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

public interface IMemberRelationshipService
{
    Task UpdateDenormalizedRelationshipFields(Member member, CancellationToken cancellationToken);
}
