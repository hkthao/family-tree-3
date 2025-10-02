using backend.Application.Common.Models;
using backend.Application.Members.Queries;
using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

public interface IMemberService : IBaseCrudService<Member, MemberDto>
{
    Task<Result<List<MemberDto>>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<Result<PaginatedList<MemberDto>>> SearchAsync(MemberFilterModel filter);
    Task<Result<MemberDto>> GetMemberDtoByIdAsync(Guid id);
}