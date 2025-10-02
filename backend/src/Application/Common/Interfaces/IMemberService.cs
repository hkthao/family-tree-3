using backend.Application.Members;
using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

public interface IMemberService : IBaseCrudService<Member, MemberDto>
{
}