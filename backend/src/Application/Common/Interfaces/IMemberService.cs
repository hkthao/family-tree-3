using backend.Application.Common.Models;
using backend.Application.Members;
using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

public interface IMemberService : IBaseCrudService<Member, MemberDto>
{
    Task<Result<PaginatedList<MemberDto>>> SearchAsync(MemberFilterModel filter);
}