using backend.Domain.Entities;
using backend.Domain.Common.Interfaces;

namespace backend.Application.Common.Interfaces;

public interface IMemberRepository : IRepository<Member>
{
    // Add any member-specific repository methods here if needed
    Task<int> CountMembersByFamilyIdAsync(Guid familyId);
}