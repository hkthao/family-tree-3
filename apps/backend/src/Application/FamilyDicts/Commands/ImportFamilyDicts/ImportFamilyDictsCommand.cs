using backend.Domain.Enums;
using backend.Application.Common.Models; // Added
using MediatR; // Added

namespace backend.Application.FamilyDicts.Commands.ImportFamilyDicts;

public record ImportFamilyDictsCommand : IRequest<Result<IEnumerable<Guid>>>
{
    public IEnumerable<FamilyDictImportDto> FamilyDicts { get; init; } = new List<FamilyDictImportDto>();
}

public record FamilyDictImportDto
{
    public string Name { get; init; } = null!;
    public FamilyDictType Type { get; init; }
    public string Description { get; init; } = null!;
    public FamilyDictLineage Lineage { get; init; }
    public bool SpecialRelation { get; init; }
    public NamesByRegionImportDto NamesByRegion { get; init; } = null!;
}

public record NamesByRegionImportDto
{
    public string North { get; init; } = null!;
    public object Central { get; init; } = null!; // Can be string or array of strings
    public object South { get; init; } = null!; // Can be string or array of strings
}
