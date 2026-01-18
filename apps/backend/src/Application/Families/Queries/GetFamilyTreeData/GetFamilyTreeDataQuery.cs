using backend.Application.Common.Models;

namespace backend.Application.Families.Queries.GetFamilyTreeData;

public record GetFamilyTreeDataQuery(
    Guid FamilyId,
    Guid? InitialMemberId
) : IRequest<Result<FamilyTreeDataDto>>;
