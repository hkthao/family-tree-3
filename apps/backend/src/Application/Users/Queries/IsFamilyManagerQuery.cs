using backend.Application.Common.Models;

namespace backend.Application.Users.Queries;

public record IsFamilyManagerQuery(Guid FamilyId) : IRequest<Result<bool>>;
