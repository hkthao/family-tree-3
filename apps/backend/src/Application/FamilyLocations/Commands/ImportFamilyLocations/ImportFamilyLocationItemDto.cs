using backend.Domain.Enums;

namespace backend.Application.FamilyLocations.Commands.ImportFamilyLocations;

public record ImportFamilyLocationItemDto
{
    public string LocationName { get; set; } = null!;
    public string? LocationDescription { get; set; }
    public double? LocationLatitude { get; set; }
    public double? LocationLongitude { get; set; }
    public string? LocationAddress { get; set; }
    public LocationType LocationType { get; set; }
    public LocationAccuracy LocationAccuracy { get; set; }
    public LocationSource LocationSource { get; set; }
}
