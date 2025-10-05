using backend.Application.Members.Queries.GetMembers;

namespace backend.Application.Members.Queries.GetMembersByIds;

public record GetMembersByIdsQuery(List<Guid> Ids) : IRequest<Result<IReadOnlyList<MemberListDto>>>;