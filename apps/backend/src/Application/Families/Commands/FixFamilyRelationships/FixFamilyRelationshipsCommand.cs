using backend.Application.Common.Models;

namespace backend.Application.Families.Commands.FixFamilyRelationships;

public record FixFamilyRelationshipsCommand : IRequest<Result>
{
    public Guid FamilyId { get; init; }
}
