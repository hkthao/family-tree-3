using backend.Application.Common.Models;

namespace backend.Application.Users.Queries;

public record IsAdminQuery() : IRequest<Result<bool>>;
