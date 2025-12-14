using backend.Application.Common.Models; // Added
using backend.Domain.Enums;

namespace backend.Application.FamilyDicts.Commands.CreateFamilyDict;

public record CreateFamilyDictCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = null!;
    public FamilyDictType Type { get; set; }
    public string Description { get; set; } = null!;
    public FamilyDictLineage Lineage { get; set; }
    public bool SpecialRelation { get; set; }
    public NamesByRegionCommandDto NamesByRegion { get; set; } = null!;
}

public class NamesByRegionCommandDto
{
    public string North { get; set; } = null!;
    public object Central { get; set; } = null!; // Can be string or array of strings
    public object South { get; set; } = null!; // Can be string or array of strings
}
