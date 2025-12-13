
using backend.Application.Common.Models;

namespace backend.Application.Identity.Queries;

public record GetUsersByIdsQuery(List<Guid> Ids) : IRequest<Result<List<UserDto>>>;
