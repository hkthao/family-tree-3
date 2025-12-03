using backend.Domain.Enums;
using backend.Application.Common.Models; // Added

namespace backend.Application.FamilyDicts.Commands.UpdateFamilyDict;

public record UpdateFamilyDictCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public FamilyDictType Type { get; set; }
    public string Description { get; set; } = null!;
    public FamilyDictLineage Lineage { get; set; }
    public bool SpecialRelation { get; set; }
    public NamesByRegionUpdateCommandDto NamesByRegion { get; set; } = null!;
}

public class NamesByRegionUpdateCommandDto
{
    public string North { get; set; } = null!;
    public object Central { get; set; } = null!; // Can be string or array of strings
    public object South { get; set; } = null!; // Can be string or array of strings
}
