
using backend.Application.Common.Models;
using backend.Application.Users.Queries;

public record GetUsersByIdsQuery(List<Guid> Ids) : IRequest<Result<List<UserDto>>>;
