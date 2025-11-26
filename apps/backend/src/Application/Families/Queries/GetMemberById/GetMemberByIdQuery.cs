using backend.Application.Common.Models;

namespace backend.Application.Members.Queries.GetMemberById;

public record GetMemberByIdQuery(Guid Id) : IRequest<Result<MemberDetailDto>>;
