using backend.Application.Common.Models;

namespace backend.Application.Members.Queries.ExportMembers;

public record ExportMembersQuery(Guid FamilyId) : IRequest<Result<string>>;
