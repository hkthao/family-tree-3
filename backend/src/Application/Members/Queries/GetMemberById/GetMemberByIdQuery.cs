namespace backend.Application.Members.Queries.GetMemberById;

public record GetMemberByIdQuery(Guid Id) : IRequest<MemberDetailDto>;