using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers; // Added missing using directive

namespace backend.Application.Members.Queries.GetMembersByFamilyId;

public record GetMembersByFamilyIdQuery(Guid FamilyId) : IRequest<Result<List<MemberListDto>>>;
