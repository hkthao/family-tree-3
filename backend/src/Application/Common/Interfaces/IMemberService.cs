using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

public interface IMemberService
{
    Task<List<Member>> GetAllMembersAsync();
    Task<Member?> GetMemberByIdAsync(Guid id);
    Task<List<Member>> GetMembersByIdsAsync(IEnumerable<Guid> ids);
    Task<Member> CreateMemberAsync(Member member);
    Task UpdateMemberAsync(Member member);
    Task DeleteMemberAsync(Guid id);
}