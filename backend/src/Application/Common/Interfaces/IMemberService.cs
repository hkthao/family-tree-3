using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

public interface IMemberService : IBaseCrudService<Member>
{
    Task<Result<List<Member>>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<Result<PaginatedList<Member>>> SearchAsync(MemberFilterModel filter);
}