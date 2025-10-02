namespace backend.Application.Members.Queries.GetMembersByIds;

public record GetMembersByIdsQuery(List<Guid> Ids) : IRequest<List<MemberDto>>;
